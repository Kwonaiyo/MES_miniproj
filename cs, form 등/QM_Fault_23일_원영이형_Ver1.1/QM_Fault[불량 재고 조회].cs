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
* 수정이력 :
* 
* 
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
            _gridUtil.InitColumnUltraGrid(grid1, "CHK",            "불량확정 여부", GridColDataType_emu.CheckBox,   120, HAlign.Center, true,  false);
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

            // << 불량원인 콤보박스 세팅 필요 >>

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
    }
}
