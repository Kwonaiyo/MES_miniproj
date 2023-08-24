using DC00_assm;
using DC00_PuMan;
using DC00_WinForm;
using Infragistics.Win.UltraWinGrid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Infragistics.UltraChart.Resources.Appearance;
using Infragistics.Win.UltraWinChart;

namespace KDTB_FORMS
{
    public partial class QM_ErrorInquire : DC00_WinForm.BaseMDIChildForm
    {
        string sdates = string.Empty;
        string sdatee = string.Empty;
        public QM_ErrorInquire()
        {
            InitializeComponent();
        }

        #region < MEMBER AREA >
        DataTable rtnDtTemp = new DataTable(); // 
        UltraGridUtil _GridUtil = new UltraGridUtil();  //그리드 객체 생성 
        string plantCode = LoginInfo.PlantCode;

        #endregion

        private void QM_ErrorInquire_Load(object sender, EventArgs e)
        {
            #region ▶ GRID ◀
            _GridUtil.InitializeGrid(this.grid1);

            _GridUtil.InitColumnUltraGrid(grid1, "PLANTCODE"   , "공장"           , GridColDataType_emu.VarChar, 150, Infragistics.Win.HAlign.Left , true, false);
            _GridUtil.InitColumnUltraGrid(grid1, "TOTBADQTY"   , "총 불량수량"    , GridColDataType_emu.Double , 100, Infragistics.Win.HAlign.Right, true, false);
            _GridUtil.InitColumnUltraGrid(grid1, "QTYOFROH"    , "원자재 불량수량", GridColDataType_emu.Double , 100, Infragistics.Win.HAlign.Right, true, false);
            _GridUtil.InitColumnUltraGrid(grid1, "QTYOFPROCESS", "공정불량수량"   , GridColDataType_emu.Double , 100, Infragistics.Win.HAlign.Right, true, false);
            _GridUtil.InitColumnUltraGrid(grid1, "ROHRATE"     , "원자재 불량률"  , GridColDataType_emu.VarChar, 100, Infragistics.Win.HAlign.Right, true, false);
            _GridUtil.InitColumnUltraGrid(grid1, "PROCESSRATE" , "공정 불량률"    , GridColDataType_emu.VarChar, 100, Infragistics.Win.HAlign.Right, true, false);
            _GridUtil.SetInitUltraGridBind(grid1);

            _GridUtil.InitializeGrid(this.grid2);

            _GridUtil.InitColumnUltraGrid(grid2, "PLANTCODE" , "공장"                , GridColDataType_emu.VarChar, 150, Infragistics.Win.HAlign.Left , true, false);
            _GridUtil.InitColumnUltraGrid(grid2, "WKCD"      , "작업장코드"          , GridColDataType_emu.VarChar, 150, Infragistics.Win.HAlign.Left , false, false);
            _GridUtil.InitColumnUltraGrid(grid2, "WKNM"      , "작업장"              , GridColDataType_emu.VarChar, 100, Infragistics.Win.HAlign.Left , true, false);
            _GridUtil.InitColumnUltraGrid(grid2, "QTY_A"     , "공장별 불량"         , GridColDataType_emu.Double , 100, Infragistics.Win.HAlign.Right, false, false);
            _GridUtil.InitColumnUltraGrid(grid2, "QTY_B"     , "공장별 작업장별 불량", GridColDataType_emu.Double , 150, Infragistics.Win.HAlign.Right, true, false);
            _GridUtil.InitColumnUltraGrid(grid2, "ERRRATE"   , "불량률"              , GridColDataType_emu.VarChar, 100, Infragistics.Win.HAlign.Right, true, false);
            _GridUtil.SetInitUltraGridBind(grid2);

            _GridUtil.InitializeGrid(this.grid3);

            _GridUtil.InitColumnUltraGrid(grid3, "PLANTCODE", "공장"           , GridColDataType_emu.VarChar, 150, Infragistics.Win.HAlign.Left, true, false);
            _GridUtil.InitColumnUltraGrid(grid3, "CUSTNAME" , "거래처"         , GridColDataType_emu.VarChar, 100, Infragistics.Win.HAlign.Left, true, false);
            _GridUtil.InitColumnUltraGrid(grid3, "TOTBADQTY", "불량수량"       , GridColDataType_emu.Double , 100, Infragistics.Win.HAlign.Right, true, false);
            _GridUtil.InitColumnUltraGrid(grid3, "QTYROH"   , "원자재 불량수량", GridColDataType_emu.Double , 100, Infragistics.Win.HAlign.Right, true, false);
            _GridUtil.InitColumnUltraGrid(grid3, "ERRRATE"  , "불량률"         , GridColDataType_emu.VarChar, 100, Infragistics.Win.HAlign.Right, true, false);
            _GridUtil.SetInitUltraGridBind(grid3);
            #endregion

            DataTable rtnDtTemp = new DataTable();
            #region ▶ COMBOBOX ◀
            rtnDtTemp = Common.StandardCODE("PLANTCODE");  // 사업장
            Common.FillComboboxMaster(this.cboPlantCode, rtnDtTemp);
            UltraGridUtil.SetComboUltraGrid(this.grid1, "PLANTCODE", rtnDtTemp);

            #endregion



            #region ▶ ENTER-MOVE ◀
            cboPlantCode.Value = plantCode;

            // 이번 달(현재)의 1일부터 나오도록 세팅.+
            dtpStart.Value = string.Format("{0:yyyy-MM-01}", DateTime.Now);
            dtpEnd.Value = string.Format("{0:yyyy-MM-dd}", DateTime.Now);
            #endregion

        }

        public override void DoInquire()
        {
            DoFind();
        }

        private void DoFind()
        {
            DBHelper helper = new DBHelper(false);
            try
            {
                _GridUtil.Grid_Clear(grid1);
                _GridUtil.Grid_Clear(grid2);
                _GridUtil.Grid_Clear(grid3);
                string sPlantCode      = DBHelper.nvlString(this.cboPlantCode.Value);
                string sStartDate      = string.Format("{0:yyyy-MM-dd}", dtpStart.Value);
                string sEndDate        = string.Format("{0:yyyy-MM-dd}", dtpEnd.Value);
                sdates = sStartDate;
                sdatee = sEndDate;


                rtnDtTemp = helper.FillTable("TGR_QM_ErrorInquire_S1", CommandType.StoredProcedure
                                             , helper.CreateParameter("@PLANTCODE"     , sPlantCode)
                                             , helper.CreateParameter("@STARTDATE"     , sStartDate)
                                             , helper.CreateParameter("@ENDDATE"       , sEndDate)
                                             );

                grid1.DataSource = rtnDtTemp;
                // this.ClosePrgForm();    // 상태 창 닫기?

                if (grid1.Rows.Count == 0)
                {
                    ShowDialog("조회할 데이터가 없습니다.");
                    return;
                }

                


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

        #region < METHOD >
        /// <summary>
        /// 조회 버튼
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            DoInquire();
        }


        #endregion

        private void grid1_AfterRowActivate(object sender, EventArgs e)
        {
            // 공장, 원자재불량수량(QTYOFROH), 공정불량수량(QTYOFPROCESS)
            string sPlantCode      = DBHelper.nvlString(this.cboPlantCode.Value);             // 공장

            DBHelper helper = new DBHelper();
            try
            {
                rtnDtTemp = helper.FillTable("TGR_QM_ErrorInquire_S2", CommandType.StoredProcedure
                                             , helper.CreateParameter("@PLANTCODE"     , sPlantCode)
                                             , helper.CreateParameter("@STARTDATE"     , sdates)
                                             , helper.CreateParameter("@ENDDATE"       , sdatee)
                                             );

                grid2.DataSource = rtnDtTemp;
                /*
                rtnDtTemp = helper.FillTable("TGR_QM_ErrorInquire_S3", CommandType.StoredProcedure
                                             , helper.CreateParameter("@PLANTCODE", sPlantCode)
                                             , helper.CreateParameter("@STARTDATE", sdates)
                                             , helper.CreateParameter("@ENDDATE", sdatee)
                                             );

                grid3.DataSource = rtnDtTemp;
                */
            }
            catch(Exception ex)
            {
                ShowDialog(ex.ToString());
            }
            finally
            {
                helper.Close();
            }

        }
    }
}
