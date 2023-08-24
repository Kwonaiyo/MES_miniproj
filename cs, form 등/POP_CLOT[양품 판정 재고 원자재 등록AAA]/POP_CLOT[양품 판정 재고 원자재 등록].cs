using System; 
using System.Data;
using DC00_PuMan;
using System.Windows.Forms;
using DC00_assm;
using Infragistics.Win;

namespace DC_POPUP
{
    public partial class POP_CLOT : DC00_WinForm.BasePopupForm
    {
        #region [ 선언자 ]
        //그리드 객체 생성
        UltraGridUtil _GridUtil = new UltraGridUtil();

        //임시로 사용할 데이터테이블 생성
        DataTable _DtTemp = new DataTable();
        string[] sClotno;

        #endregion

        public POP_CLOT(DataTable DT)
        {
            InitializeComponent();
            
            _DtTemp = DT;
        }

        private void POP_CLOT_Load(object sender, EventArgs e)
        {
            _GridUtil.InitializeGrid(this.grid1);

            _GridUtil.InitColumnUltraGrid(grid1, "CHK",            "원자재포함여부", GridColDataType_emu.CheckBox, 120, HAlign.Center, true, true);
            _GridUtil.InitColumnUltraGrid(grid1, "PLANTCODE",      "공장"          , GridColDataType_emu.VarChar , 150, HAlign.Left,   true, false);
            _GridUtil.InitColumnUltraGrid(grid1, "WORKCENTERCODE", "작업장코드"    , GridColDataType_emu.VarChar , 170, HAlign.Left,   true, false);
            _GridUtil.InitColumnUltraGrid(grid1, "FAULTNO",        "불량판정번호"  , GridColDataType_emu.VarChar , 150, HAlign.Center, true, false);
            _GridUtil.InitColumnUltraGrid(grid1, "CLOTNO",         "원자재LOT"     , GridColDataType_emu.VarChar , 170, HAlign.Center, true, false);
            _GridUtil.SetInitUltraGridBind(grid1);

            DataTable grTemp = new DataTable();
            // 공장
            grTemp = Common.StandardCODE("PLANTCODE");
            UltraGridUtil.SetComboUltraGrid(grid1, "PLANTCODE", grTemp);

            // 작업장
            grTemp = Common.GET_Workcenter_Code();
            UltraGridUtil.SetComboUltraGrid(grid1, "WORKCENTERCODE", grTemp);

            grid1.DataSource = _DtTemp;

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            DataTable dtTemp = new DataTable();
            DataTable dtTempA = new DataTable();
            dtTempA.Columns.Add("CLOTNO");

            int iCount = 0;

            dtTemp = grid1.chkChange();
            if ( dtTemp == null || dtTemp.Rows.Count == 0)
            {
                MessageBox.Show("선택된 항목이 없습니다. 확인 후 다시 시도해주세요.");
                return;
            }
            foreach (DataRow dr in dtTemp.Rows)
            {
                if (Convert.ToString(dr["CHK"]) == "0")
                {
                    continue;
                }
                iCount++;
                dtTempA.Rows.Add(dr["CLOTNO"]);

                
                // sClotno = dr["CLOTNO"].ToString();
            }

            sClotno = new string[iCount];
            for( int i = 0; i < sClotno.Length; i++)
            {
                sClotno[i] = Convert.ToString(dtTempA.Rows[i]["CLOTNO"]);
            }

            MessageBox.Show("등록이 완료되었습니다.");
            this.Close();
            // 원자재LOT 정보를 날려보내주는 로직 구현해야함
            // 체크 안됐을때 리턴 해주고
        }
        public string[] RET()
        {
            return sClotno;
        }
    }
    
}
