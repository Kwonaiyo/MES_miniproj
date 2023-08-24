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
* 작성일자 : 2023-05-19
* 설    명 : 생산 실적 등록
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
            _gridUtil.InitColumnUltraGrid(grid1, "PLANTCODE",      "공장",           GridColDataType_emu.VarChar,    100, HAlign.Left,   false, false);
            _gridUtil.InitColumnUltraGrid(grid1, "WORKCENTERCODE", "작업장",         GridColDataType_emu.VarChar,    130, HAlign.Left,   false, false);
            _gridUtil.InitColumnUltraGrid(grid1, "WORKCENTERNAME", "작업장",         GridColDataType_emu.VarChar,    150, HAlign.Left,   true,  false);
            _gridUtil.InitColumnUltraGrid(grid1, "ORDERNO",        "작업지시번호",   GridColDataType_emu.VarChar,    160, HAlign.Center, true,  false);
            _gridUtil.InitColumnUltraGrid(grid1, "ITEMCODE",       "생산품목",       GridColDataType_emu.VarChar,    130, HAlign.Left,   true,  false);
            _gridUtil.InitColumnUltraGrid(grid1, "ITEMNAME",       "품명",           GridColDataType_emu.VarChar,    150, HAlign.Left,   true,  false);
            _gridUtil.InitColumnUltraGrid(grid1, "ORDERQTY",       "지시수량",       GridColDataType_emu.Double,     120, HAlign.Right,  true,  false);
            _gridUtil.InitColumnUltraGrid(grid1, "PRODQTY",        "양품수량",       GridColDataType_emu.Double,     120, HAlign.Right,  true,  false);
            _gridUtil.InitColumnUltraGrid(grid1, "BADQTY",         "불량수량",       GridColDataType_emu.Double,     120, HAlign.Right,  true,  false);
            _gridUtil.InitColumnUltraGrid(grid1, "UNITCODE",       "단위",           GridColDataType_emu.VarChar,     80, HAlign.Center, true,  false);
            _gridUtil.InitColumnUltraGrid(grid1, "WORKSTATUSCODE", "가동비가동코드", GridColDataType_emu.VarChar,    100, HAlign.Left,   false, false);
            _gridUtil.InitColumnUltraGrid(grid1, "WORKSTATUS",     "상태",           GridColDataType_emu.VarChar,    100, HAlign.Center, true,  false);
            _gridUtil.InitColumnUltraGrid(grid1, "MATLOTNO",       "투입 LOT",       GridColDataType_emu.VarChar,    220, HAlign.Center, true,  false);
            _gridUtil.InitColumnUltraGrid(grid1, "COMPONENTQTY",   "투입잔량",       GridColDataType_emu.Double,     150, HAlign.Right,  true,  false);
            _gridUtil.InitColumnUltraGrid(grid1, "WORKER",         "작업자코드",     GridColDataType_emu.VarChar,    100, HAlign.Left,   false, false);
            _gridUtil.InitColumnUltraGrid(grid1, "WORKERNAME",     "작업자",         GridColDataType_emu.VarChar,    100, HAlign.Center, true,  false);
            _gridUtil.InitColumnUltraGrid(grid1, "ORDERSTARTDATE", "지시시작일시",   GridColDataType_emu.DateTime24, 180, HAlign.Center, true,  false);
            _gridUtil.InitColumnUltraGrid(grid1, "ORDERENDDATE",   "지시종료일시",   GridColDataType_emu.DateTime24, 180, HAlign.Center, true,  false);
            _gridUtil.SetInitUltraGridBind(grid1);

            // 공장
            dtTemp = Common.StandardCODE("PLANTCODE");
            Common.FillComboboxMaster(cboPlantCode, dtTemp);

            // 단위
            dtTemp = Common.StandardCODE("UNITCODE");
            UltraGridUtil.SetComboUltraGrid(grid1, "UNITCODE", dtTemp);

            // 작업장
            dtTemp = Common.GET_Workcenter_Code();
            Common.FillComboboxMaster(cboWorkcenterCode, dtTemp);

            // 작업자 팝업
            BizTextBoxManager biztxt = new BizTextBoxManager();
            biztxt.PopUpAdd(txtItemCode, txtItemName, "WORKER_MASTER");

            // 공장 기본값 세팅
            cboPlantCode.Value = LoginInfo.PlantCode;
        }

        public override void DoInquire()
        {
            doInquire();
        }
        private void doInquire()
        {
            string sWorkcentercode = string.Empty;
            if (grid1.Rows.Count != 0 && grid1.ActiveRow != null)
            {
                sWorkcentercode = Convert.ToString(grid1.ActiveRow.Cells["WORKCENTERCODE"].Value);
            }

            DBHelper helper = new DBHelper();
            try
            {
                string sPlantCode      = Convert.ToString(cboPlantCode.Value);      // 공장
                string sWorkcenterCode = Convert.ToString(cboWorkcenterCode.Value); // 작업장

                dtTemp = helper.FillTable("SP06_QM_Fault_S1", CommandType.StoredProcedure, helper.CreateParameter("@PLANTCODE",      sPlantCode)
                                                                                                , helper.CreateParameter("@WORKCENTERCODE", sWorkcenterCode));
                grid1.DataSource = dtTemp;
                if (grid1.Rows.Count == 0)
                {
                    ShowDialog("조회할 데이터가 없습니다.");
                    return;
                }

                // 그리드에 데이터가 조회되었을 경우 그리드 수만큼 돌면서 먼저 선택한 행을 찾는 로직
                for (int i = 0; i < grid1.Rows.Count; i++)
                {
                    if (Convert.ToString(grid1.Rows[i].Cells["WORKCENTERCODE"].Value) == sWorkcentercode)
                    {
                        grid1.Rows[i].Selected  = true;
                        grid1.Rows[i].Activated = true;
                        return;
                    }
                }
                // 기존에 선택한 작업장이 그리드에 표현되지 않았을 때
                grid1.Rows[0].Selected  = true;
                grid1.Rows[0].Activated = true;

            }
            catch (Exception ex)
            { ShowDialog(ex.ToString()); }
            finally
            { helper.Close(); }
        }

        #region < 1. 작업자 등록 로직 >
        private void btnWorkerReg_Click(object sender, EventArgs e)
        {
            // 작업장이 선택되었는지 확인
            if (grid1.ActiveRow == null) return;

            // 작업자를 조회했는지 확인
            string sWorkerId = txtItemCode.Text; // 작업자 ID
            if (sWorkerId.Trim() == "")
            {
                ShowDialog("작업자를 선택하지 않았습니다.");
                return;
            }

            DBHelper helper = new DBHelper(true);

            try
            {
                string sPlantCode      = Convert.ToString(grid1.ActiveRow.Cells["PLANTCODE"].Value);      // 공장
                string sWorkcenterCode = Convert.ToString(grid1.ActiveRow.Cells["WORKCENTERCODE"].Value); // 작업장

                helper.ExecuteNoneQuery("SP06_QM_Fault_I1", CommandType.StoredProcedure, helper.CreateParameter("@PLANTCODE",      sPlantCode)
                                                                                              , helper.CreateParameter("@WORKCENTERCODE", sWorkcenterCode)
                                                                                              , helper.CreateParameter("@WORKERID",       sWorkerId));
                if (helper.RSCODE != "S")
                {
                    helper.Rollback();
                    ShowDialog("작업자 등록 실패\r\n" + helper.RSMSG);
                    return;
                }
                helper.Commit();
                ShowDialog("작업자 등록 완료");
                doInquire();
            }
            catch (Exception ex)
            {
                helper.Rollback();
                ShowDialog(ex.ToString());
            }
            finally { helper.Close(); }
        }
        #endregion

        #region < 2. 작업지시 선택 >
        private void btnOrderSelect_Click(object sender, EventArgs e)
        {
            // 선택한 작업장이 있는지 확인
            if (grid1.Rows.Count == 0)   return;
            if (grid1.ActiveRow == null) return;

            // 작업자가 등록된 상태인지 확인
            string sWorkerId = Convert.ToString(grid1.ActiveRow.Cells["WORKER"].Value);
            if (sWorkerId == "")
            {
                ShowDialog("작업자 등록이 안됨요");
                return;
            }
            string sWorkStatus = Convert.ToString(grid1.ActiveRow.Cells["WORKSTATUSCODE"].Value);
            if (sWorkStatus == "R")  // S : Stop, R : Run
            {
                ShowDialog("현재 작업장의 상태가 가동 중입니다.\r\n비가동 등록 후 진행하세요.");
                return;
            }

            // 투입된 LOT 여부 확인
            string sMatlotno = Convert.ToString(grid1.ActiveRow.Cells["MATLOTNO"].Value);
            if (sMatlotno != "")
            {
                ShowDialog("투입된 LOT 잔량이 있읍니다.\r\nLOT 투입 취소 후 진행해줘잉.");
                return;
            }

            // 작업지시 선택 팝업 호출
            string sWorkcenterCode = Convert.ToString(grid1.ActiveRow.Cells["WORKCENTERCODE"].Value);
            string sWorkcenterName = Convert.ToString(grid1.ActiveRow.Cells["WORKCENTERNAME"].Value);
            string sPlantCode      = Convert.ToString(grid1.ActiveRow.Cells["PLANTCODE"].Value);

            POP_ORDERNO popOrderNo = new POP_ORDERNO(sWorkcenterCode, sWorkcenterName);
            popOrderNo.ShowDialog();

            // 작업지시 팝업에서 선택한 작업지시를 작업장 현재 상태로 등록
            string sOrderNo = Convert.ToString(popOrderNo.Tag);
            if (sOrderNo == "") return;

            if (ShowDialog("작업지시 변경을 적용하시겠습니까?") == DialogResult.Cancel) return;

            // 작업지시 등록
            DBHelper helper = new DBHelper(true);
            try
            {
                helper.ExecuteNoneQuery("SP06_QM_Fault_I2", CommandType.StoredProcedure, helper.CreateParameter("@PLANTCODE",      sPlantCode)
                                                                                              , helper.CreateParameter("@WORKCENTERCODE", sWorkcenterCode)
                                                                                              , helper.CreateParameter("@ORDERNO",        sOrderNo)
                                                                                              , helper.CreateParameter("@WORKER",         sWorkerId));
                if (helper.RSCODE != "S")
                {
                    helper.Rollback();
                    ShowDialog(helper.RSMSG);
                    return;
                }
                helper.Commit();
                ShowDialog("작업지시 변경 완료");
                doInquire();
            }
            catch (Exception ex)
            {
                helper.Rollback();
                ShowDialog(ex.ToString());
            }
            finally
            {
                helper.Close();
            }
        }
        #endregion

        #region < 3. 그리드의 행 선택 시 (작업장 선택 시) 데이터 표현 >
        private void grid1_AfterRowActivate(object sender, EventArgs e)
        {
            // 작업자 정보 표시
            txtItemCode.Text   = Convert.ToString(grid1.ActiveRow.Cells["WORKER"].Value);
            txtItemName.Text = Convert.ToString(grid1.ActiveRow.Cells["WORKERNAME"].Value);

            // LOT 투입 여부에 따른 투입/취소 변경
            string sLotNo = Convert.ToString(grid1.ActiveRow.Cells["MATLOTNO"].Value);
            txtInLotNo.Text = sLotNo;
            if (sLotNo == "")
            {
                btnLotInOut.Text = "(4) LOT 투입";
                btnLotInOut.Tag = true;
            }
            else
            {
                btnLotInOut.Text = "(4) LOT 투입 취소";
                btnLotInOut.Tag = false;
            }

            // 가동 / 비가동 여부에 따른 버튼 상태 변경
            string sRunStop = Convert.ToString(grid1.ActiveRow.Cells["WORKSTATUSCODE"].Value);
            if (sRunStop == "R")
            {
                btnRunStop.Text = "(5) 비가동";
                btnRunStop.Tag = false;
            }
            else
            {
                btnRunStop.Text = "(5) 가동";
                btnRunStop.Tag = true;
            }
        }
        #endregion

        #region < 4. LOT 투입 >
        private void btnLotInOut_Click(object sender, EventArgs e)
        {
            if (grid1.ActiveRow == null) return;

            string sPlantCode      = Convert.ToString(grid1.ActiveRow.Cells["PLANTCODE"].Value);
            string sWorkcenterCode = Convert.ToString(grid1.ActiveRow.Cells["WORKCENTERCODE"].Value);
            string sOrderNo        = Convert.ToString(grid1.ActiveRow.Cells["ORDERNO"].Value);
            string sItemCode       = Convert.ToString(grid1.ActiveRow.Cells["ITEMCODE"].Value);
            string sUnitCode       = Convert.ToString(grid1.ActiveRow.Cells["UNITCODE"].Value);
            string sWorker         = Convert.ToString(grid1.ActiveRow.Cells["WORKER"].Value);
            string sLotNo          = txtInLotNo.Text;

            if (sOrderNo == "")
            {
                ShowDialog("작업지시번호가 없습니다");
                return;
            }
            if (sWorker == "")
            {
                ShowDialog("작업자를 선택해줘잉");
                return;
            }
            
            // LOT 투입/취소 판정
            string sLotInOutFlag = "IN"; // LOT 투입 상태
            if (!Convert.ToBoolean(btnLotInOut.Tag)) sLotInOutFlag = "OUT";

            // 원자재 LOT 투입 / 취소
            DBHelper helper = new DBHelper(true);
            try
            {
                helper.ExecuteNoneQuery("SP06_QM_Fault_I3", CommandType.StoredProcedure, helper.CreateParameter("@PLANTCODE",      sPlantCode)      // 공장
                                                                                              , helper.CreateParameter("@ITEMCODE",       sItemCode)       // 투입 품목
                                                                                              , helper.CreateParameter("@LOTNO",          sLotNo)          // 투입 원자재 LOTNO
                                                                                              , helper.CreateParameter("@WORKCENTERCODE", sWorkcenterCode) // 작업장
                                                                                              , helper.CreateParameter("@ORDERNO",        sOrderNo)        // 작업지시번호
                                                                                              , helper.CreateParameter("@UNITCODE",       sUnitCode)       // 투입 단위
                                                                                              , helper.CreateParameter("@INFLAG",         sLotInOutFlag)   // IN : LOT 투입, OUT : LOT 투입 취소
                                                                                              , helper.CreateParameter("@WORKER",         sWorker));       // 작업자
                if (helper.RSCODE != "S")
                {
                    helper.Rollback();
                    ShowDialog(helper.RSMSG);
                    return;
                }
                helper.Commit();
                ShowDialog("정상적으로 원자재 LOT 투입(취소)을 완료했다네");
                doInquire();
            }
            catch (Exception ex)
            {
                helper.Rollback();
                ShowDialog(ex.ToString());
            }
            finally
            {
                helper.Close();
            }
        }
        #endregion

        #region < 5. 작업장 가동 / 비가동 등록 >
        private void ultraButton1_Click(object sender, EventArgs e)
        {
            if (grid1.ActiveRow == null) return;

            string sPlantCode      = Convert.ToString(grid1.ActiveRow.Cells["PLANTCODE"].Value);
            string sWorkcenterCode = Convert.ToString(grid1.ActiveRow.Cells["WORKCENTERCODE"].Value);
            string sOrderNo        = Convert.ToString(grid1.ActiveRow.Cells["ORDERNO"].Value);
            string sWorker         = Convert.ToString(grid1.ActiveRow.Cells["WORKER"].Value);

            if (sOrderNo == "")
            {
                ShowDialog("작업지시번호가 없습니다");
                return;
            }
            if (sWorker == "")
            {
                ShowDialog("작업자를 선택해줘잉");
                return;
            }

            string sRunStop = "R";
            if (!Convert.ToBoolean(btnRunStop.Tag)) sRunStop = "S";

            DBHelper helper = new DBHelper(true);
            try
            {
                helper.ExecuteNoneQuery("SP06_QM_Fault_I4", CommandType.StoredProcedure, helper.CreateParameter("@PLANTCODE",      sPlantCode)      // 공장
                                                                                              , helper.CreateParameter("@WORKCENTERCODE", sWorkcenterCode) // 작업장
                                                                                              , helper.CreateParameter("@ORDERNO",        sOrderNo)        // 작업지시번호
                                                                                              , helper.CreateParameter("@RUNSTOPFLAG",    sRunStop)        // 가동여부
                                                                                              , helper.CreateParameter("@WORKER",         sWorker));       // 작업자
                if (helper.RSCODE != "S")
                {
                    helper.Rollback();
                    ShowDialog(helper.RSMSG);
                    return;
                }
                helper.Commit();
                ShowDialog("정상적으로 (비)가동을 등록했다네");
                doInquire();
            }
            catch (Exception ex)
            {
                helper.Rollback();
                ShowDialog(ex.ToString());
            }
            finally
            {
                helper.Close();
            }
        }
        #endregion

        #region < 6. 생산실적 등록 >
        private void btnProdReg_Click(object sender, EventArgs e)
        {
            if (grid1.ActiveRow == null) return;

            double dProdQty   = 0; // 누적 양품 수량
            double dErrorQty  = 0; // 누적 불량 수량
            double dTProdQty  = 0; // 입력한 양품 수량
            double dTErrorQty = 0; // 입력한 불량 수량
            double dOrderQty  = 0; // 작업 지시 수량

            // 누적 양품 수량
            string sProdQty = Convert.ToString(grid1.ActiveRow.Cells["PRODQTY"].Value).Replace(",","");
            Double.TryParse(sProdQty, out dProdQty);

            // 누적 불량 수량
            string sErrorQty = Convert.ToString(grid1.ActiveRow.Cells["BADQTY"].Value).Replace(",","");
            Double.TryParse(sErrorQty, out dErrorQty);

            // 입력한 양품 수량
            string sTProdQty = Convert.ToString(txtProdQty.Text);
            Double.TryParse(sTProdQty, out dTProdQty);

            // 입력한 불량 수량
            string sTErrorQty = Convert.ToString(txtBadQty.Text);
            Double.TryParse(sTErrorQty, out dTErrorQty);

            // 작업 지시 수량
            string sOrderQty = Convert.ToString(grid1.ActiveRow.Cells["ORDERQTY"].Value).Replace(",", "");
            Double.TryParse(sOrderQty, out dOrderQty);

            // 투입 LOT 정보
            string sMatLotNo       = Convert.ToString(grid1.ActiveRow.Cells["MATLOTNO"].Value);
            if (sMatLotNo == "")
            {
                ShowDialog("투입 LOT의 정보가 없습니다.\r\nLOT 투입 후 진행하세요");
                return;
            }

            // 수량의 입력 여부
            if (dTProdQty + dTErrorQty == 0)
            {
                ShowDialog("생산실적 정보를 입력하지 않았습니다.\r\n입력 후 진행하세요");
                return;
            }

            // 지시수량보다 많은 생산수량을 입력했는지 확인
            if (dOrderQty < dProdQty + dTProdQty)
            {
                ShowDialog("작업지시수량보다 많은 수량을 입력했습니다.\r\n확인 후 진행하세요");
                return;
            }

            string sPlantCode      = Convert.ToString(grid1.ActiveRow.Cells["PLANTCODE"].Value);
            string sWorkcenterCode = Convert.ToString(grid1.ActiveRow.Cells["WORKCENTERCODE"].Value);
            string sOrderNo        = Convert.ToString(grid1.ActiveRow.Cells["ORDERNO"].Value);
            string sItemCode       = Convert.ToString(grid1.ActiveRow.Cells["ITEMCODE"].Value);
            string sUnitCode       = Convert.ToString(grid1.ActiveRow.Cells["UNITCODE"].Value);

            // 생산실적 등록 SP
            DBHelper helper = new DBHelper(true);
            try
            {
                helper.ExecuteNoneQuery("SP06_QM_Fault_I5", CommandType.StoredProcedure, helper.CreateParameter("@PLANTCODE",      sPlantCode)      // 공장
                                                                                              , helper.CreateParameter("@WORKCENTERCODE", sWorkcenterCode) // 작업장
                                                                                              , helper.CreateParameter("@ORDERNO",        sOrderNo)        // 작업지시번호
                                                                                              , helper.CreateParameter("@ITEMCODE",       sItemCode)       // 품목
                                                                                              , helper.CreateParameter("@UNITCODE",       sUnitCode)       // 단위
                                                                                              , helper.CreateParameter("@PRODQTY",        sTProdQty)       // 입력한 양품 수량
                                                                                              , helper.CreateParameter("@BADQTY",         sTErrorQty)      // 입력한 불량 수량
                                                                                              , helper.CreateParameter("@MATLOTNO",       sMatLotNo));     // 투입한 LOT 정보
                if (helper.RSCODE != "S")
                {
                    helper.Rollback();
                    ShowDialog(helper.RSMSG);
                    return;
                }
                helper.Commit();
                ShowDialog("정상적으로 생산실적을 등록했다네");
                doInquire();
                txtProdQty.Text = "";
                txtBadQty.Text  = "";
                PrintBarcode(sPlantCode, helper.RSMSG, helper);
            }
            catch (Exception ex)
            {
                helper.Rollback();
                ShowDialog(ex.ToString());
            }
            finally
            {
                helper.Close();
            }
        }
        #endregion

        /// <summary>
        /// 제품 바코드 발행 로직
        /// </summary>
        private void PrintBarcode(string sPlantCode, string sLotNo, DBHelper helper)
        {
            if (sLotNo == "") return;

            // 제품 LOT의 정보 가져오기
            dtTemp = helper.FillTable("SP06_QM_Fault_S2", CommandType.StoredProcedure, helper.CreateParameter("@PLANTCODE", sPlantCode)
                                                                                            , helper.CreateParameter("@LOTNO", sLotNo));
            if (dtTemp.Rows.Count == 0) return;

            // 바코드 발행 로직

            // 1. 제품 바코드 디자인 선언
            Report_LotBacodeFERT LOT_FERT = new Report_LotBacodeFERT();

            // 2. 레포트 북 선언
            ReportBook reportBook = new ReportBook();
            
            // 3. 제품 바코드에 DataTable 바인딩
            LOT_FERT.DataSource = dtTemp;

            // 4. 레포트 북에 바코드 디자인 객체 추가
            reportBook.Reports.Add(LOT_FERT);

            // 5. 미리보기 창에 레포트 북 전달
            ReportViewer reportViewer = new ReportViewer(reportBook, 1);
            reportViewer.ShowDialog();
        }

        #region < 7. 작업지시 종료 >
        private void btnOrderClose_Click(object sender, EventArgs e)
        {
            if (grid1.ActiveRow == null) return;

            if (Convert.ToString(grid1.ActiveRow.Cells["MATLOTNO"].Value) != "")
            {
                ShowDialog("LOT 투입을 취소 후 진행하세요.");
                return;
            }
            if (Convert.ToString(grid1.ActiveRow.Cells["WORKSTATUSCODE"].Value) == "R")
            {
                ShowDialog("작업장이 현재 가동중입니다. 비가동 등록 후 진행하세요.");
                return;
            }

            string sPlantCode      = Convert.ToString(grid1.ActiveRow.Cells["PLANTCODE"].Value);
            string sWorkcenterCode = Convert.ToString(grid1.ActiveRow.Cells["WORKCENTERCODE"].Value);
            string sOrderNo        = Convert.ToString(grid1.ActiveRow.Cells["ORDERNO"].Value);

            DBHelper helper = new DBHelper(true);
            try
            {
                helper.ExecuteNoneQuery("SP06_QM_Fault_I6", CommandType.StoredProcedure, helper.CreateParameter("@PLANTCODE",      sPlantCode)      // 공장
                                                                                              , helper.CreateParameter("@WORKCENTERCODE", sWorkcenterCode) // 작업장
                                                                                              , helper.CreateParameter("@ORDERNO",        sOrderNo));      // 작업지시번호
                if (helper.RSCODE != "S")
                {
                    helper.Rollback();
                    ShowDialog(helper.RSMSG);
                    return;
                }
                helper.Commit();
                ShowDialog("정상적으로 작업지시를 종료했다네");
                doInquire();
                txtProdQty.Text = "";
                txtBadQty.Text = "";
            }
            catch (Exception ex)
            {
                helper.Rollback();
                ShowDialog(ex.ToString());
            }
            finally
            {
                helper.Close();
            }
        }
        #endregion
    }
}
