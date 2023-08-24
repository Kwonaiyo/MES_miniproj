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
using System.Text.RegularExpressions;
using System.Threading;
using System.Collections;
using DC00_Component;
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
        DataTable dt_A   = new DataTable();
        UltraGridUtil _gridUtil = new UltraGridUtil();
        string[] sClot;
        public QM_Fault()
        {
            InitializeComponent();
        }

        private void QM_Fault_Load(object sender, EventArgs e)
        {
            // 생산 실적 그리드 세팅
            _gridUtil.InitializeGrid(grid1);
            _gridUtil.InitColumnUltraGrid(grid1, "CHK",            "재검사 여부",  GridColDataType_emu.CheckBox,   100, HAlign.Center, true, true);
            _gridUtil.InitColumnUltraGrid(grid1, "PLANTCODE",      "공장",         GridColDataType_emu.VarChar,    160, HAlign.Left,   true, false);
            _gridUtil.InitColumnUltraGrid(grid1, "WORKCENTERCODE", "작업장",       GridColDataType_emu.VarChar,    130, HAlign.Left,   true, false);
            _gridUtil.InitColumnUltraGrid(grid1, "WORKCENTERNAME", "작업장명",     GridColDataType_emu.VarChar,    110, HAlign.Left,   true, false);
            _gridUtil.InitColumnUltraGrid(grid1, "ITEMCODE",       "품목",         GridColDataType_emu.VarChar,    130, HAlign.Left,   true, false);
            _gridUtil.InitColumnUltraGrid(grid1, "ITEMNAME",       "품명",         GridColDataType_emu.VarChar,    130, HAlign.Left,   true, false);
            _gridUtil.InitColumnUltraGrid(grid1, "FAULTNO",        "불량판정번호", GridColDataType_emu.VarChar,    160, HAlign.Center, true, false);
            _gridUtil.InitColumnUltraGrid(grid1, "FAULTQTY",       "불량수량",     GridColDataType_emu.Double,      80, HAlign.Right,  true, false);
            _gridUtil.InitColumnUltraGrid(grid1, "UNITCODE",       "단위",         GridColDataType_emu.VarChar,     80, HAlign.Left,   true, false);
            _gridUtil.InitColumnUltraGrid(grid1, "MAKEDATE",       "등록일시",     GridColDataType_emu.DateTime24, 180, HAlign.Center, true, false);
            _gridUtil.InitColumnUltraGrid(grid1, "MAKER",          "등록자",       GridColDataType_emu.VarChar,     60, HAlign.Center, true, false);
            _gridUtil.SetInitUltraGridBind(grid1);
            grid1.UpdateMode = UpdateMode.OnCellChange;

            // 공장
            dtTemp = Common.StandardCODE("PLANTCODE");
            Common.FillComboboxMaster(cboPlantCode, dtTemp);
            UltraGridUtil.SetComboUltraGrid(grid1, "PLANTCODE", dtTemp);

            // 품목 팝업
            BizTextBoxManager biztxt = new BizTextBoxManager();
            biztxt.PopUpAdd(txtItemCode, txtItemName, "ITEM_MASTER", new object[] { cboPlantCode.Value, "FERT" });

            // 단위
            dtTemp = Common.StandardCODE("UNITCODE");
            UltraGridUtil.SetComboUltraGrid(grid1, "UNITCODE", dtTemp);

            // 불량원인 콤보박스 세팅.. 불량품이 없을 경우, 빈칸으로 표시되도록 빈 칸 추가.
            dtTemp = Common.StandardCODE("FAULT_WHY");
            Common.FillComboboxMaster(cboBadWhy, dtTemp, "CODE_ID", "CODE_NAME", "", "");

            // 공장 기본값 세팅
            cboPlantCode.Value = LoginInfo.PlantCode;
            dtpStart.Value = string.Format("{0:yyyy-MM-01}", DateTime.Now);
            dtpEnd.Value   = string.Format("{0:yyyy-MM-dd}", DateTime.Now);

            dt_A.Columns.Add("CHK");
            dt_A.Columns.Add("PLANTCODE");
            dt_A.Columns.Add("WORKCENTERCODE");
            dt_A.Columns.Add("WORKCENTERNAME");
            dt_A.Columns.Add("FAULTNO");
            dt_A.Columns.Add("CLOTNO");
        }

        #region < 0. 기본 세팅 메소드 >
        private void txtProdQty_KeyPress(object sender, KeyPressEventArgs e)
        {
            int key = (int)e.KeyChar;
            if ((key < 48 || key > 57) && key != 8 && key != 46) // 아스키코드 48~57(0~9), 8(백스페이스), 46(.)만 허용
            { e.Handled = true; }
            if (key == 46) // 아스키코드 46(.)일 경우에는 중복 X (한 번만 허용)
            {
                if (string.IsNullOrEmpty(txtProdQty.Text) || txtProdQty.Text.Contains('.') == true)
                { e.Handled = true; }
            }
        }
        private void txtBadQty_KeyPress(object sender, KeyPressEventArgs e)
        {
            int key = (int)e.KeyChar;
            if ((key < 48 || key > 57) && key != 8 && key != 46) // 아스키코드 48~57(0~9), 8(백스페이스), 46(.)만 허용. 나머지 키는 입력불가
            { e.Handled = true; }
            if (key == 46) // 아스키코드 46(.)일 경우에는 중복 X (한 번만 허용)
            {
                if (string.IsNullOrEmpty(txtBadQty.Text) || txtBadQty.Text.Contains('.') == true)
                { e.Handled = true; }
            }
        }
        private void grid1_MouseDown(object sender, MouseEventArgs e)
        {
            double dTotalQty = 0;

            foreach (DataRow dr in dtTemp.Rows)
            {
                if (Convert.ToString(dr["CHK"]) == "0") continue;
                dTotalQty += Convert.ToInt32(dr["FAULTQTY"]);
            }
            txtTotalQty.Text = dTotalQty.ToString();
        }
        #endregion

        #region < 1. 조회버튼 클릭 >
        public override void DoInquire()
        { doInquire(); }
        private void doInquire()
        {
            txtProdQty.Text = "";
            txtBadQty.Text = "";
            txtProdReason.Text = "";
            cboBadWhy.Text = "";
            txtBadReason.Text = "";
            DBHelper helper = new DBHelper();
            try
            {
                string sPlantCode = Convert.ToString(cboPlantCode.Value);            // 공장
                string sItemCode  = Convert.ToString(txtItemCode.Text);              // 품목
                string sStartdate = string.Format("{0:yyyy-MM-dd}", dtpStart.Value); // 조회 시작일자
                string sEndDate   = string.Format("{0:yyyy-MM-dd}", dtpEnd.Value);   // 조회 종료일자

                dtTemp = helper.FillTable("TGR_QM_Fault_S1", CommandType.StoredProcedure
                                                           , helper.CreateParameter("PLANTCODE", sPlantCode)
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

        #region < 2. 저장버튼 클릭 >
        public override void DoSave()
        {
            doSave();
        }
        public void doSave()
        {
            // grid에 표시된 내용이 없을 경우 return;
            if (grid1.Rows.Count == 0) return;

            // 체크박스 선택된 행이 없을 경우 return;
            dtTemp = new DataTable();
            dtTemp = grid1.chkChange();
            if (dtTemp == null || dtTemp.Rows.Count == 0)
            {
                ShowDialog("선택된 행이 없습니다. 다시 확인 후 시도해주세요.");
                return;
            }

            double dSumofFaultQty = 0;
            double dtxtProdQty; // txtProdQty를 double로 받아올 변수
            double dtxtBadQty;  // txtBadQty를 double로 받아올 변수
            if (String.IsNullOrEmpty(txtProdQty.Text) == true) dtxtProdQty = 0; // TextBox의 값이 null일 경우 예외처리
            else dtxtProdQty = Convert.ToDouble(txtProdQty.Text);
            if (String.IsNullOrEmpty(txtBadQty.Text)  == true) dtxtBadQty = 0;
            else dtxtBadQty  = Convert.ToDouble(txtBadQty.Text);

            if (dtxtProdQty == 0 && dtxtBadQty == 0)
            {
                ShowDialog("수량정보가 입력되지 않았습니다. 확인 후 다시 시도해주세요.");
                return;
            }

            dt_A.Clear();
            int iCHK_Length = 0; // 선택된 행의 개수를 저장할 변수

            // 체크한 데이터 행들의 불량수량의 합이 검사결과 등록할 양품수량 + 불량수량의 값과 다르면 return;
            foreach (DataRow dr in dtTemp.Rows)
            {
                if (Convert.ToString(dr["CHK"]) == "0") continue;
                string st = dr["FAULTNO"].ToString();
                string sClotno = get_CLOTNO_FROMDB(st);     // st를 가지고 DB에서 st(FAULTNO)에 대응되는 CLOTNO를 가져옴.
                dt_A.Rows.Add("0", dr["PLANTCODE"], dr["WORKCENTERCODE"], dr["WORKCENTERNAME"], dr["FAULTNO"], sClotno);    // 가져온 CLOTNO들을 dt_A 데이터테이블에 추가
                dSumofFaultQty += Convert.ToDouble(dr["FAULTQTY"]);
                iCHK_Length++;
            }

            if (dSumofFaultQty != dtxtProdQty + dtxtBadQty)
            {
                ShowDialog("검사 수량과 양품/불량 입력 수량이 일치하지 않습니다. 확인 후 다시 시도해주세요.");
                return;
            }

            if (dtxtProdQty > 0) SHOMETHECLOTPOPUP();

            DBHelper helper = new DBHelper(true);
            try
            {
                int iDel_Count = 1;
                string sProdReason = Convert.ToString(txtProdReason.Text);
                string sBadWhy     = Convert.ToString(cboBadWhy.Value);
                string sBadReason  = Convert.ToString(txtBadReason.Text);

                foreach (DataRow dr in dtTemp.Rows)
                {
                    if (Convert.ToString(dr["CHK"]) == "0") continue;

                    helper.ExecuteNoneQuery("TGR_QM_Fault_I1", CommandType.StoredProcedure
                                                             , helper.CreateParameter("PLANTCODE",  dr["PLANTCODE"])  // 공장
                                                             , helper.CreateParameter("FAULTNO",    dr["FAULTNO"])    // 불량판정번호
                                                             , helper.CreateParameter("PRODQTY",    dtxtProdQty)      // 양품수량
                                                             , helper.CreateParameter("FAULTQTY",   dr["FAULTQTY"])   // 불량수량
                                                             , helper.CreateParameter("PRODREASON", sProdReason)      // 양품사유
                                                             , helper.CreateParameter("DELWHY",     sBadWhy)          // 불량원인
                                                             , helper.CreateParameter("DELREASON",  sBadReason)       // 불량사유
                                                             , helper.CreateParameter("MAKER",      LoginInfo.UserID) // 등록자
                                                             , helper.CreateParameter("DELQTY",     dtxtBadQty)       // 폐기수량
                                                             , helper.CreateParameter("DELSEQ",     iDel_Count)       // 현재 행의 수
                                                             , helper.CreateParameter("CHK_LENGTH", iCHK_Length));    // 행의 총 길이
                    if (helper.RSCODE != "S")
                    {
                        helper.Rollback();
                        ShowDialog(helper.RSMSG);
                        return;
                    }
                    iDel_Count += 1;
                }
                string newLotNo = helper.RSMSG;

                if (sClot == null) return;
                for (int i = 0; i < sClot.Length; i++)
                {
                    string sCLOT = sClot[i];
                    helper.ExecuteNoneQuery("TGR_QM_Fault_I2", CommandType.StoredProcedure
                                                             , helper.CreateParameter("NEWLOTNO", newLotNo)  // 양품판정된 LOTNO
                                                             , helper.CreateParameter("CLOTNO", sCLOT)
                                                             , helper.CreateParameter("WORKER", LoginInfo.UserID));
                    if (helper.RSCODE != "S")
                    {
                        helper.Rollback();
                        ShowDialog(helper.RSMSG);
                        return;
                    }
                }
                helper.Commit();
                txtProdQty.Text    = "";
                txtBadQty.Text     = "";
                txtProdReason.Text = "";
                cboBadWhy.Text     = "";
                txtBadReason.Text  = "";
                ShowDialog("재검사 결과를 등록 완료했습니다.");
                doInquire();
            }
            catch (Exception ex)
            {
                helper.Rollback();
                ShowDialog(ex.ToString());
            }
            finally
            { helper.Close(); }
        }
        #endregion

        #region < 3. 원자재 선택 팝업 >
        private void SHOMETHECLOTPOPUP()
        {
            //sClot.Initialize();
            if (dt_A.Rows.Count == 0) return;

            string st = string.Empty;
            DataView dv = dt_A.DefaultView;
            dv.Sort = "CLOTNO ASC";
            DataTable sort_dt_A = dv.ToTable();


            st = Convert.ToString(sort_dt_A.Rows[0]["CLOTNO"]);  // CLOTNO
            DataTable dtTempAAA = new DataTable();
            dtTempAAA = sort_dt_A.Clone();
            dtTempAAA.ImportRow(sort_dt_A.Rows[0]);
            for (int i = 1; i < sort_dt_A.Rows.Count; i++)
            {
                if (Convert.ToString(sort_dt_A.Rows[i]["CLOTNO"]) == st)
                {
                    continue;
                }
                else
                {
                    st = Convert.ToString(sort_dt_A.Rows[i]["CLOTNO"]);
                    dtTempAAA.ImportRow(sort_dt_A.Rows[i]);
                }
            }
            POP_CLOT test = new POP_CLOT(dtTempAAA);
            test.ShowDialog();
            sClot = test.RET();
        }

        public string get_CLOTNO_FROMDB(string st)
        {
            string msg = Get_CLOTNO(st);
            return msg;
        }
        private string Get_CLOTNO(string st)
        {
            DBHelper helper = new DBHelper();
            string sFaultNO = st;

            helper.ExecuteNoneQuery("TGR_GET_CLOT", CommandType.StoredProcedure, helper.CreateParameter("FAULTNO", sFaultNO));

            string msg = helper.RSMSG;
            helper.Close();

            return msg;
        }
        #endregion
    }
}
