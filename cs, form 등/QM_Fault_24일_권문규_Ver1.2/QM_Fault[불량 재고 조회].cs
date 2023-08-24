using DC_POPUP;
using DC00_assm;
using DC00_PuMan;
using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Telerik.Reporting;
/*********************************************************************
* 화면  ID : QM_Fault
* 작 성 자 : 정원영
* 작성일자 : 2023-06-23
* 설    명 : 불량 재고 조회
*********************************************************************
* 수정이력 : 불량원인 콤보박스 세팅 완료.. 불량품이 없을 경우, 빈칸으로 표시될 수 있도록 세팅 완료..
*            QM_Fault[불량재고조회] 폼에 '불량수량 확정' 그룹박스 이름을 '검사결과 등록' 으로 변경.
*            QM_Fault 폼에 '불량수량 변경' 버튼 Text를 '등록' 으로 변경, Name을 btnConfirm으로 변경.
*            등록 버튼 클릭 시 실행되는 로직 구현 중
* 수 정 자 : 권문규
* 수정일자 : 2023-06-24
*********************************************************************/

namespace KDTB_FORMS
{
    public partial class QM_Fault : DC00_WinForm.BaseMDIChildForm
    {
        DataTable dtTemp = new DataTable();
        UltraGridUtil _gridUtil = new UltraGridUtil();
        public QM_Fault()
        {
            InitializeComponent();
        }

        private void QM_Fault_Load(object sender, EventArgs e)
        {
            // 생산 실적 그리드 세팅
            _gridUtil.InitializeGrid(grid1);
            _gridUtil.InitColumnUltraGrid(grid1, "CHK",            "불량확정 여부", GridColDataType_emu.CheckBox,   120, HAlign.Center, true,  true);
            _gridUtil.InitColumnUltraGrid(grid1, "PLANTCODE",      "공장",          GridColDataType_emu.VarChar,    100, HAlign.Left,   true,  false);
            _gridUtil.InitColumnUltraGrid(grid1, "WORKCENTERCODE", "작업장",        GridColDataType_emu.VarChar,    130, HAlign.Left,   false, false);
            _gridUtil.InitColumnUltraGrid(grid1, "WORKCENTERNAME", "작업장명",      GridColDataType_emu.VarChar,    150, HAlign.Left,   true,  false);
            _gridUtil.InitColumnUltraGrid(grid1, "ITEMCODE",       "품목",          GridColDataType_emu.VarChar,    130, HAlign.Left,   true,  false);
            _gridUtil.InitColumnUltraGrid(grid1, "ITEMNAME",       "품명",          GridColDataType_emu.VarChar,    150, HAlign.Left,   true,  false);
            _gridUtil.InitColumnUltraGrid(grid1, "FAULTQTY",       "불량수량",      GridColDataType_emu.Double,     120, HAlign.Right,  true,  false);
            _gridUtil.InitColumnUltraGrid(grid1, "UNITCODE",       "단위",          GridColDataType_emu.VarChar,     80, HAlign.Center, true,  false);
            _gridUtil.InitColumnUltraGrid(grid1, "MAKEDATE",       "등록일시",      GridColDataType_emu.DateTime24, 180, HAlign.Center, true,  false);
            _gridUtil.InitColumnUltraGrid(grid1, "MAKER",          "등록자",        GridColDataType_emu.VarChar,    100, HAlign.Left,   true,  false);
            _gridUtil.SetInitUltraGridBind(grid1);

            // 공장
            dtTemp = Common.StandardCODE("PLANTCODE");
            Common.FillComboboxMaster(cboPlantCode, dtTemp);

            // 품목 팝업
            BizTextBoxManager biztxt = new BizTextBoxManager();
            biztxt.PopUpAdd(txtItemCode, txtItemName, "ITEM_MASTER");

            // 단위
            dtTemp = Common.StandardCODE("UNITCODE");
            UltraGridUtil.SetComboUltraGrid(grid1, "UNITCODE", dtTemp);

            // 불량원인 콤보박스 세팅.. 불량품이 없을 경우, 빈칸으로 표시되도록 빈 칸 추가.
            dtTemp = Common.StandardCODE("TEST_WHY");
            Common.FillComboboxMaster(cboBadWhy, dtTemp, "CODE_ID", "CODE_NAME", "", "");

            // 공장 기본값 세팅
            cboPlantCode.Value = LoginInfo.PlantCode;
            dtpStart.Value = string.Format("{0:yyyy-MM-01}", DateTime.Now);
            dtpEnd.Value   = string.Format("{0:yyyy-MM-dd}", DateTime.Now);

        }

        #region < 1. 조회버튼 클릭 >
        public override void DoInquire()
        {
            doInquire();
        }
        private void doInquire()
        {
            DBHelper helper = new DBHelper();
            try
            {
                string sPlantCode = Convert.ToString(cboPlantCode.Value); // 공장
                string sItemCode  = Convert.ToString(txtItemCode.Text);   // 품목
                string sStartdate = string.Format("{0:yyyy-MM-dd}", dtpStart.Value);
                string sEndDate   = string.Format("{0:yyyy-MM-dd}", dtpEnd.Value);

                dtTemp = helper.FillTable("TGR_QM_Fault_S1", CommandType.StoredProcedure, helper.CreateParameter("PLANTCODE", sPlantCode)
                                                                                        , helper.CreateParameter("ITEMCODE",  sItemCode)
                                                                                        , helper.CreateParameter("STARTDATE", sStartdate)
                                                                                        , helper.CreateParameter("ENDDATE",   sEndDate));
                grid1.DataSource = dtTemp;
                if (grid1.Rows.Count == 0)
                {
                    ShowDialog("조회할 데이터가 없습니다.");
                    return;
                }
            }
            catch (Exception ex)
            { ShowDialog(ex.ToString()); }
            finally
            { helper.Close(); }
        }
        #endregion

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            // grid에 표시된 내용이 없을 경우 return;
            if(grid1.Rows.Count == 0)
            { 
                return;
            }

            // 체크박스 체크된 행이 없을 경우 return;
            DataTable dtTemp = new DataTable();
            dtTemp = grid1.chkChange();
            if (dtTemp.Rows.Count == 0)
            {
                ShowDialog("체크된 행이 없습니다. 다시 확인 후 시도해주세요.");
                return;
            }

            // < txtProdQty와 txtBadQty에 숫자가 아닌 값이 들어왔을때 return하는 로직을 넣고싶은데 생각이 안남 >>

            // 체크한 데이터 행들의 불량수량의 합이 검사결과 등록할 양품수량 + 불량수량의 값과 다르면 return;
            foreach (DataRow dt in dtTemp.Rows)
            {
                int iSumofFaultqty = 0;
                iSumofFaultqty += Convert.ToInt32(dt["FAULTQTY"]);
                if (iSumofFaultqty != Convert.ToInt32(txtProdQty) + Convert.ToInt32(txtBadQty))
                {
                    ShowDialog("검사 수량과 양품/불량 입력 수량이 일치하지 않습니다. 확인 후 다시 시도해주세요.");
                    return;
                }
            }
            

        }
    }
}
