#region < HEADER AREA >
// *---------------------------------------------------------------------------------------------*
//   Form ID      : QM_FaultRec
//   Form Name    : 공정 재고 입출 이력
//   Name Space   : KDTB_FORMS
//   Created Date : 2023/05
//   Made By      : DSH
//   Description  : 
// *---------------------------------------------------------------------------------------------*
#endregion

#region < USING AREA >
using System;
using System.Data;
using DC_POPUP;

using DC00_assm;
using DC00_WinForm;
using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
#endregion

namespace KDTB_FORMS
{
    public partial class QM_FaultRec : DC00_WinForm.BaseMDIChildForm
    {

        #region < MEMBER AREA > 
        UltraGridUtil _GridUtil = new UltraGridUtil();  //그리드 객체 생성 
        DataTable rtnDtTemp = new DataTable(); // 
        #endregion


        #region < CONSTRUCTOR >
        public QM_FaultRec()
        {
            InitializeComponent();
        }
        #endregion


        #region < FORM EVENTS >
        private void QM_FaultRec_Load(object sender, EventArgs e)
        { 
            string plantCode        = LoginInfo.PlantCode;

            #region ▶ GRID ◀
            _GridUtil.InitializeGrid(this.grid1);
            _GridUtil.InitColumnUltraGrid(grid1, "PLANTCODE",      "공장",         GridColDataType_emu.VarChar,    140, HAlign.Left,   true,  false);
            _GridUtil.InitColumnUltraGrid(grid1, "FAULTNO",        "불량판정번호", GridColDataType_emu.VarChar,    130, HAlign.Center, true,  false);
            _GridUtil.InitColumnUltraGrid(grid1, "INOUTDATE",      "입/출일자",    GridColDataType_emu.VarChar,    100, HAlign.Center, true,  false);
            _GridUtil.InitColumnUltraGrid(grid1, "WORKCENTERCODE", "작업장",       GridColDataType_emu.VarChar,    120, HAlign.Center, false, false);
            _GridUtil.InitColumnUltraGrid(grid1, "WORKCENTERNAME", "작업장명",     GridColDataType_emu.VarChar,    120, HAlign.Center, false, false);
            _GridUtil.InitColumnUltraGrid(grid1, "ITEMCODE",       "품목",         GridColDataType_emu.VarChar,    120, HAlign.Left,   true,  false);
            _GridUtil.InitColumnUltraGrid(grid1, "ITEMNAME",       "품명",         GridColDataType_emu.VarChar,    120, HAlign.Left,   true,  false);
            _GridUtil.InitColumnUltraGrid(grid1, "WHCODE",         "창고",         GridColDataType_emu.VarChar,    180, HAlign.Left,   true,  false);
            _GridUtil.InitColumnUltraGrid(grid1, "INOUTCODE",      "입출유형",     GridColDataType_emu.VarChar,    120, HAlign.Left,   true,  false);
            _GridUtil.InitColumnUltraGrid(grid1, "INOUTFLAG",      "입출구분",     GridColDataType_emu.VarChar,     80, HAlign.Left,   true,  false);
            _GridUtil.InitColumnUltraGrid(grid1, "FAULTQTY",       "불량수량",     GridColDataType_emu.Double,      80, HAlign.Right,  true,  false);
            _GridUtil.InitColumnUltraGrid(grid1, "UNITCODE",       "단위",         GridColDataType_emu.VarChar,     70, HAlign.Left,   true,  false);
            _GridUtil.InitColumnUltraGrid(grid1, "MAKER",          "등록자",       GridColDataType_emu.VarChar,     60, HAlign.Center, true,  false);
            _GridUtil.InitColumnUltraGrid(grid1, "MAKEDATE",       "등록일시",     GridColDataType_emu.DateTime24, 150, HAlign.Center, true,  false);
            _GridUtil.SetInitUltraGridBind(grid1); 
            #endregion

            #region ▶ COMBOBOX ◀
            rtnDtTemp = Common.StandardCODE("PLANTCODE");  // 사업장
            Common.FillComboboxMaster(this.cboPlantCode, rtnDtTemp);
            UltraGridUtil.SetComboUltraGrid(this.grid1, "PLANTCODE", rtnDtTemp);

            rtnDtTemp = Common.StandardCODE("UNITCODE");     //단위
            UltraGridUtil.SetComboUltraGrid(this.grid1, "UNITCODE", rtnDtTemp);
             
            rtnDtTemp = Common.Get_ItemCode(new string[] { "ROH", "FERT", "HALB" });
            Common.FillComboboxMaster(this.cboItemCode, rtnDtTemp);

            rtnDtTemp = Common.StandardCODE("WHCODE");     //창고
            UltraGridUtil.SetComboUltraGrid(this.grid1, "WHCODE", rtnDtTemp);

            rtnDtTemp = Common.StandardCODE("INOUTCODE", "RELCODE1 = 'TGR'");     //입출유형
            Common.FillComboboxMaster(this.cboInOutCode, rtnDtTemp);
            UltraGridUtil.SetComboUltraGrid(this.grid1, "INOUTCODE", rtnDtTemp);

            rtnDtTemp = Common.StandardCODE("INOUTFLAG");     //입출 구분 
            UltraGridUtil.SetComboUltraGrid(this.grid1, "INOUTFLAG", rtnDtTemp); // 입출유형,구분 나중에 넣어야함

            #endregion

            #region ▶ ENTER-MOVE ◀
            cboPlantCode.Value = plantCode;
            dtpStart.Value = string.Format("{0:yyyy-MM-01}", DateTime.Now);
            dtpEnd.Value   = string.Format("{0:yyyy-MM-dd}", DateTime.Now);
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
                _GridUtil.Grid_Clear(grid1);
                string sPlantCode = Convert.ToString(this.cboPlantCode.Value);
                string sItemCode  = Convert.ToString(this.cboItemCode.Value);
                string sInOutCode = Convert.ToString(this.cboInOutCode.Value);
                string sFaultNo   = Convert.ToString(txtLotNo.Text);
                string sStartdate = string.Format("{0:yyyy-MM-dd}", dtpStart.Value);
                string sEndDate   = string.Format("{0:yyyy-MM-dd}", dtpEnd.Value);

                rtnDtTemp = helper.FillTable("TGR_QM_FaultRec_S1", CommandType.StoredProcedure
                                                                 , helper.CreateParameter("PLANTCODE", sPlantCode)
                                                                 , helper.CreateParameter("ITEMCODE",  sItemCode)
                                                                 , helper.CreateParameter("INOUTCODE", sInOutCode)
                                                                 , helper.CreateParameter("FAULTNO",   sFaultNo)
                                                                 , helper.CreateParameter("STARTDATE", sStartdate)
                                                                 , helper.CreateParameter("ENDDATE",   sEndDate));
                 
                this.grid1.DataSource = rtnDtTemp;
               
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
        #endregion 
    }
}




