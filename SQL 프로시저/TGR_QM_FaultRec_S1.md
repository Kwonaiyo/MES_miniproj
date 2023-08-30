```SQL
USE [KDTB03_MES_2]
GO
/****** Object:  StoredProcedure [dbo].[TGR_QM_FaultRec_S1]    Script Date: 2023-08-30 오전 11:44:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		정원영
-- Create date: 2023-06-24
-- Description:	불량 재고 입출 이력 조회
-- =============================================
ALTER PROCEDURE [dbo].[TGR_QM_FaultRec_S1]
	 @PLANTCODE VARCHAR(10) -- 공장
	,@ITEMCODE	VARCHAR(30)	-- 품목
	,@FAULTNO	VARCHAR(30)	-- 불량판정번호
	,@INOUTCODE VARCHAR(30) -- 입출유형
	,@STARTDATE	VARCHAR(10)	-- 입출조회 시작일자
	,@ENDDATE	VARCHAR(10)	-- 입출조회 종료일자

	,@LANG		VARCHAR(10) = 'KO'
	,@RS_CODE	VARCHAR(1)   OUTPUT
	,@RS_MSG	VARCHAR(200) OUTPUT
AS
BEGIN
	SELECT A.PLANTCODE 						 AS PLANTCODE 	   -- 공장
		  ,A.FAULTNO  						 AS FAULTNO  	   -- 불량판정번호
		  ,A.INOUTDATE						 AS INOUTDATE	   -- 입/출일자
		  ,A.WORKCENTERCODE					 AS WORKCENTERCODE -- 작업장
		  ,B.WORKCENTERNAME					 AS WORKCENTERNAME -- 작업장명
		  ,A.ITEMCODE						 AS ITEMCODE	   -- 품목
		  ,C.ITEMNAME						 AS ITEMNAME	   -- 품명
		  ,A.WHCODE							 AS WHCODE		   -- 창고
		  ,A.INOUTCODE                       AS INOUTCODE      -- 입출유형
		  ,A.INOUTFLAG						 AS INOUTFLAG	   -- 입출구분
		  ,A.QTY  				 			 AS FAULTQTY  	   -- 불량수량
		  ,A.UNITCODE						 AS UNITCODE	   -- 단위
		  ,CONVERT(VARCHAR, A.MAKEDATE, 120) AS MAKEDATE	   -- 등록일시
		  ,DBO.FN_GET_USERNAME(A.MAKER)		 AS MAKER 		   -- 등록자
	  FROM TB_FaultRec A WITH(NOLOCK) LEFT JOIN TB_WorkCenterMaster B
											 ON A.PLANTCODE		 = B.PLANTCODE
											AND A.WORKCENTERCODE = B.WORKCENTERCODE
									  LEFT JOIN TB_ItemMaster C
											 ON A.PLANTCODE = C.PLANTCODE
											AND A.ITEMCODE  = C.ITEMCODE

	 WHERE A.PLANTCODE LIKE '%' + @PLANTCODE
	   AND A.ITEMCODE  LIKE '%' + @ITEMCODE
	   AND A.FAULTNO   LIKE '%' + @FAULTNO + '%'
	   AND A.INOUTCODE LIKE '%' + @INOUTCODE
	   AND A.INOUTDATE BETWEEN @STARTDATE AND @ENDDATE
  ORDER BY A.MAKEDATE, A.FAULTNO, A.INOUTCODE
END
```