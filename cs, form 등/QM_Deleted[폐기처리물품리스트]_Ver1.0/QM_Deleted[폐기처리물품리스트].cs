#region < HEADER AREA >
// *---------------------------------------------------------------------------------------------*
//   Form ID      : QM_Deleted
//   Form Name    : 제품창고 입고
//   Name Space   : KDTB_FORMS
//   Created Date : 2023/05
//   Made By      : DSH
//   Description  : 
// *---------------------------------------------------------------------------------------------*
#endregion

#region < USING AREA >
using System;
using System.Data;
using System.Linq.Expressions;
using DC_POPUP;
using DC00_assm;
using DC00_Component;
using DC00_PuMan;
using DC00_WinForm;

using Infragistics.Win.UltraWinGrid;
using Infragistics.Win.UltraWinToolbars;
using Telerik.Reporting;
#endregion

namespace KDTB_FORMS
{
    public partial class QM_Deleted : DC00_WinForm.BaseMDIChildForm
    {

        #region < MEMBER AREA >
        DataTable rtnDtTemp = new DataTable(); // 
        UltraGridUtil _GridUtil = new UltraGridUtil();  //그리드 객체 생성
        string plantCode = LoginInfo.PlantCode;

        #endregion


        #region < CONSTRUCTOR >
        public QM_Deleted()
        {
            InitializeComponent();
        }
        #endregion


        #region < FORM EVENTS >
        private void QM_Deleted_Load(object sender, EventArgs e)
        {
            #region ▶ GRID ◀
            _GridUtil.InitializeGrid(this.grid1);
            _GridUtil.InitColumnUltraGrid(grid1, "CHK",       "선택"    , GridColDataType_emu.CheckBox, 80, Infragistics.Win.HAlign.Center, true, true);
            _GridUtil.InitColumnUltraGrid(grid1, "PLANTCODE", "공장"    , GridColDataType_emu.VarChar, 120, Infragistics.Win.HAlign.Left, true, false);
            _GridUtil.InitColumnUltraGrid(grid1, "ITEMCODE",  "품목"    , GridColDataType_emu.VarChar, 140, Infragistics.Win.HAlign.Left, true, false);
            _GridUtil.InitColumnUltraGrid(grid1, "ITEMNAME",  "품목명"  , GridColDataType_emu.VarChar, 140, Infragistics.Win.HAlign.Left, true, false);
            _GridUtil.InitColumnUltraGrid(grid1, "LOTNO",     "LOTNO"   , GridColDataType_emu.VarChar, 170, Infragistics.Win.HAlign.Left, true, false);
            _GridUtil.InitColumnUltraGrid(grid1, "STOCKQTY",  "재고수량", GridColDataType_emu.Double, 100, Infragistics.Win.HAlign.Right, true, false);
            _GridUtil.InitColumnUltraGrid(grid1, "UNITCODE",  "단위"    , GridColDataType_emu.VarChar, 100, Infragistics.Win.HAlign.Left, true, false);
            _GridUtil.InitColumnUltraGrid(grid1, "MAKER",     "입고자"  , GridColDataType_emu.VarChar, 100, Infragistics.Win.HAlign.Left, true, false);
            _GridUtil.InitColumnUltraGrid(grid1, "MAKEDATE",  "입고일시", GridColDataType_emu.DateTime24, 170, Infragistics.Win.HAlign.Left, true, false);
            _GridUtil.SetInitUltraGridBind(grid1);
            #endregion

            #region ▶ COMBOBOX ◀
            rtnDtTemp = Common.StandardCODE("PLANTCODE");  // 사업장
            Common.FillComboboxMaster(this.cboPlantCode, rtnDtTemp);
            UltraGridUtil.SetComboUltraGrid(this.grid1, "PLANTCODE", rtnDtTemp);

            rtnDtTemp = Common.StandardCODE("UNITCODE");     //단위
            UltraGridUtil.SetComboUltraGrid(this.grid1, "UNITCODE", rtnDtTemp);

            BizTextBoxManager bizText = new BizTextBoxManager();
            bizText.PopUpAdd(txtItemCode_H, txtItemName_H, "ITEM_MASTER");

            #endregion

            #region ▶ POP-UP ◀
            #endregion

            #region ▶ ENTER-MOVE ◀
            cboPlantCode.Value = plantCode;
            dtpStart.Value = string.Format("{0:yyyy-MM-01}", DateTime.Now);
            #endregion
        }
        #endregion


        #region < TOOL BAR AREA >
        public override void DoInquire()
        {
            DoFind();
        }
        private void DoFind()
        {
            DBHelper helper = new DBHelper(false);
            try
            {
                base.DoInquire();
                _GridUtil.Grid_Clear(grid1);
                string sPlantCode = DBHelper.nvlString(this.cboPlantCode.Value);
                string sStartDate = string.Format("{0:yyyy-MM-dd}", dtpStart.Value);
                string sEndDate = string.Format("{0:yyyy-MM-dd}", dtpEnd.Value);
                string sItemCode = DBHelper.nvlString(this.txtItemCode_H.Text);
                string sLotNo = DBHelper.nvlString(txtLotNo.Text);


                rtnDtTemp = helper.FillTable("SP00_QM_Deleted_S1", CommandType.StoredProcedure
                                    , helper.CreateParameter("@PLANTCODE", sPlantCode)
                                    , helper.CreateParameter("@ITEMCODE", sItemCode)
                                    , helper.CreateParameter("@LOTNO", sLotNo)
                                    , helper.CreateParameter("@STARTDATE", sStartDate)
                                    , helper.CreateParameter("@ENDDATE", sEndDate)
                                    );

                ClosePrgForm();
                grid1.DataSource = rtnDtTemp;
                if (grid1.Rows.Count == 0)
                    ShowDialog("조회할 데이터 가 없습니다.");
            }
            catch (Exception ex)
            {
                ShowDialog(ex.ToString(), DialogForm.DialogType.OK);
            }
            finally
            {
                helper.Close();
            }
        }

        public override void DoSave()
        {
            _doSave();
        }


        private void _doSave()
        {
            DataTable dt = grid1.chkChange();
            if (dt == null) return;

            if (ShowDialog("선택 한 제품 재고 를 제품창고 입고등록 하시겠습니까?")
                == System.Windows.Forms.DialogResult.Cancel) return;

            // 출고 등록을 시작. 
            DBHelper helper = new DBHelper(true);
            try
            {
                foreach (DataRow dr in dt.Rows)
                {

                    if (Convert.ToString(dr["CHK"]) == "0") continue;
                    helper.ExecuteNoneQuery("SP00_QM_Deleted_U1", CommandType.StoredProcedure
                                            , helper.CreateParameter("@LOTNO", dr["LOTNO"])
                                            , helper.CreateParameter("@MAKER", LoginInfo.UserID)
                                            );

                    if (helper.RSCODE != "S")
                    {
                        helper.Rollback();
                        ShowDialog(helper.RSMSG);
                        return;
                    }
                }
                helper.Commit();
                ShowDialog("정상적으로 출고 등록을 완료 하였습니다.");
                DoInquire();
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



