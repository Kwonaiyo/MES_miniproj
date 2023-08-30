# MES_miniproj
mes 미니프로젝트!
+ 이미지<br>
  - 불량 재고 조회 및 등록<br>
![불량 재고 조회 및 등록](https://github.com/Kwonaiyo/MES_miniproj/blob/main/images/%EB%B6%88%EB%9F%89%20%EC%9E%AC%EA%B3%A0%20%EC%A1%B0%ED%9A%8C%20%EB%B0%8F%20%EB%93%B1%EB%A1%9D%20%ED%99%94%EB%A9%B4.png)
  - 원자재 LOT 등록 팝업창<br>
![원자재 LOT 등록 팝업창](https://github.com/Kwonaiyo/MES_miniproj/blob/main/images/%EC%9B%90%EC%9E%90%EC%9E%AC%20LOT%20%EB%93%B1%EB%A1%9D%20%ED%8C%9D%EC%97%85%EC%B0%BD.PNG)
  - 불량재고 입출이력 조회<br>
![불량재고 입출이력 조회](https://github.com/Kwonaiyo/MES_miniproj/blob/main/images/%EB%B6%88%EB%9F%89%20%EC%9E%AC%EA%B3%A0%20%EC%9E%85%EC%B6%9C%20%EC%9D%B4%EB%A0%A5%20%EC%A1%B0%ED%9A%8C.png)
  - 불량률 조회 <br>
![불량률 조회 화면](https://github.com/Kwonaiyo/MES_miniproj/blob/main/images/%EB%B6%88%EB%9F%89%EB%A5%A0%20%EC%A1%B0%ED%9A%8C%20%ED%99%94%EB%A9%B4%20.PNG)
  - QT 화면 구성<br>
+ ! 여기가 특히 힘들었다 .. 
```cs
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
                // CLOTNO가 중복된다면 패스
                if (Convert.ToString(sort_dt_A.Rows[i]["CLOTNO"]) == st)
                {
                    continue;
                }
                // 새로운 CLOTNO가 나오면 st에 할당해주고, dtTempAAA에 정보 추가
                else
                {
                    st = Convert.ToString(sort_dt_A.Rows[i]["CLOTNO"]);
                    dtTempAAA.ImportRow(sort_dt_A.Rows[i]);
                }
            }
            POP_CLOT test = new POP_CLOT(dtTempAAA);
            test.ShowDialog();
            sClot = test.RET();  // 팝업에서 선택한 CLOTNO들이 저장되어있는 배열
        }
```
