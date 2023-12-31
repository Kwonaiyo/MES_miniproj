```SQL
USE [KDTB03_MES_2]
GO
/****** Object:  StoredProcedure [dbo].[TGR_PP_ActureOutput_I5]    Script Date: 2023-08-30 오전 11:33:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===============================================================================
-- Author:		정원영
-- Create date: 2023-06-23
-- Description:	기존 '생산실적 등록' 프로시저에 TB_Fault, TB_FaultRec INSERT 추가
-- ===============================================================================
ALTER PROCEDURE [dbo].[TGR_PP_ActureOutput_I5]
	 @PLANTCODE		 VARCHAR(10) -- 공장
	,@WORKCENTERCODE VARCHAR(10) -- 작업장
	,@ORDERNO		 VARCHAR(30) -- 작업지시번호
	,@ITEMCODE		 VARCHAR(30) -- 품목
	,@UNITCODE		 VARCHAR(10) -- 단위
	,@PRODQTY		 FLOAT		 -- 입력한 양품수량
	,@BADQTY		 FLOAT		 -- 입력한 불량수량
	,@MATLOTNO		 VARCHAR(30) -- 투입한 LOT 정보

	,@LANG			 VARCHAR(10)
	,@RS_CODE		 VARCHAR(1)	  OUTPUT
	,@RS_MSG		 VARCHAR(200) OUTPUT
AS
BEGIN
	-- 시간 정의
   DECLARE @LD_NOWDATE DATETIME
		  ,@LS_NOWDATE VARCHAR(10)
	   SET @LD_NOWDATE = GETDATE()
	   SET @LS_NOWDATE = CONVERT(VARCHAR, @LD_NOWDATE, 23)

	-- 상태테이블 변수정의
   DECLARE @LS_ORDERNO	 VARCHAR(30) -- 작업지시번호
		  ,@LS_ITEMCODE	 VARCHAR(30) -- 품목
		  ,@LS_WORKER	 VARCHAR(20) -- 작업자 정보
	SELECT @LS_ORDERNO	 = ORDERNO
		  ,@LS_ITEMCODE	 = ITEMCODE
		  ,@LS_WORKER	 = WORKER
	  FROM TP_WorkcenterStatus WITH(NOLOCK)
	 WHERE PLANTCODE	  = @PLANTCODE
	   AND WORKCENTERCODE = @WORKCENTERCODE

	-- V 체크

	-- V1. 현재 작업장 상태에 작업지시 선택되어 있는가?
	IF (ISNULL(@ORDERNO, '') = '')
	BEGIN
		SET @RS_CODE = 'E'
		SET @RS_MSG  = '작업지시 선택 필요'
		RETURN;
	END

	-- V2. 작업자를 등록한 상태인가?
	IF (ISNULL(@LS_WORKER, '') = '')
	BEGIN
		SET @RS_CODE = 'E'
		SET @RS_MSG  = '작업자 등록 필요'
		RETURN;
	END

	-- V3. BOM 수량만큼 투입 잔량이 남아있는지 체크
   DECLARE @LF_TOTALQTY  FLOAT		 -- 입력한 양품 + 입력한 불량수량
		  ,@LF_FINALQTY  FLOAT		 -- (입력한 양품 + 입력한 불량수량) * BOM 차감수량
		  ,@LF_PRODQTY	 FLOAT		 -- 입력한 양품수량 * BOM 차감수량
		  ,@LS_CITEMCODE VARCHAR(30) -- 투입 원자재 ITEMCODE
		  ,@LS_CUNITCODE VARCHAR(10) -- 투입 원자재 단위
		  ,@LF_STOCKQTY  FLOAT		 -- 투입 LOT 재공재고 잔량

	-- 입력 양품 수량 + 입력 불량 수량
	SET @LF_TOTALQTY = @PRODQTY + @BADQTY

	-- 재공재고에서 투입한 LOT의 수량 받아오기
	SELECT @LF_STOCKQTY  = STOCKQTY
		  ,@LS_CITEMCODE = ITEMCODE -- COMPONENT(원자재 ITEMCODE)
	  FROM TB_StockWIP WITH(NOLOCK)
	 WHERE PLANTCODE	  = @PLANTCODE
	   AND WORKCENTERCODE = @WORKCENTERCODE
	   AND LOTNO		  = @MATLOTNO

	-- BOM 확인하여 차감하여야 하는 총 수량 찾기
	SELECT @LF_FINALQTY = @LF_TOTALQTY * ISNULL(COMPONENTQTY, 0) -- (양품+불량) * BOM수량
		  ,@LF_PRODQTY  = @PRODQTY	   * ISNULL(COMPONENTQTY, 0) -- 양품 * BOM수량
	  FROM TB_BomMaster WITH(NOLOCK)
	 WHERE PLANTCODE = @PLANTCODE
	   AND ITEMCODE  = @ITEMCODE
	   AND COMPONENT = @LS_CITEMCODE
	   
	-- 비교
	IF (ISNULL(@LF_FINALQTY, 0) = 0)
	BEGIN
		SET @RS_CODE = 'E'
		SET @RS_MSG  = 'BOM에 등록된 차감 수량이 등록되어 있지 않습니다. 관리자와 문의하세요.'
		RETURN;
	END

	IF (@LF_STOCKQTY < @LF_FINALQTY)
	BEGIN
		SET @RS_CODE = 'E'
		SET @RS_MSG  = '투입 잔량이 부족합니다. 원자재 투입 후 진행하세요.'
		RETURN;
	END

	-- 생산 실적 등록 로직 시작

	-- 1. 작업지시 내역에 생산정보 등록
	UPDATE TB_ProductPlan
	   SET PRODQTY	= ISNULL(PRODQTY, 0) + @PRODQTY
		  ,BADQTY	= ISNULL(BADQTY,  0) + @BADQTY
		  ,EDITDATE = @LD_NOWDATE
		  ,EDITOR	= @LS_WORKER
	 WHERE PLANTCODE = @PLANTCODE
	   AND ORDERNO	 = @LS_ORDERNO

	-- 2. 가동 비가동 이력에 생산정보 등록
	UPDATE TP_WorkcenterStatusRec
	   SET PRODQTY	= ISNULL(PRODQTY, 0) + @PRODQTY
		  ,BADQTY	= ISNULL(BADQTY,  0) + @BADQTY
		  ,EDITDATE = @LD_NOWDATE
		  ,EDITOR	= @LS_WORKER
	 WHERE PLANTCODE	  = @PLANTCODE
	   AND WORKCENTERCODE = @WORKCENTERCODE
	   AND ORDERNO		  = @LS_ORDERNO
	   AND RSENDDATE IS NULL

	-- 3. LOT TRACKING 등록(양품수량이 존재할 경우에만 등록해야함)
	
	-- LOT TRACKING 테이블 변수정의
   DECLARE @LS_LOTNO VARCHAR(30) -- 완제품 LOT NO
		  ,@LI_LOTSEQ INT
	SELECT @LI_LOTSEQ = ISNULL(MAX(SEQ), 0) + 1
	  FROM TP_LotTracking WITH(NOLOCK)
	 WHERE PLANTCODE = @PLANTCODE
	   AND LOTNO	 = @LS_LOTNO

	IF (ISNULL(@PRODQTY, 0) <> 0) -- 양품수량이 존재할 경우에만 등록
	BEGIN
		-- 완제품 LOT NO 채번
		SET @LS_LOTNO = DBO.FN_LOTNO('LT_F')

		-- LOT TRACKING
		INSERT INTO TP_LotTracking (PLANTCODE,	   LOTNO,		 SEQ,	  ORDERNO,  WORKCENTERCODE,	 ITEMCODE,  PRODQTY,  UNITCODE,	   CLOTNO,	   CITEMCODE,	    INQTY,	   CUNITCODE,	 MAKEDATE,	    MAKER)
						   VALUES (@PLANTCODE, @LS_LOTNO, @LI_LOTSEQ, @LS_ORDERNO, @WORKCENTERCODE, @ITEMCODE, @PRODQTY, @UNITCODE, @MATLOTNO, @LS_CITEMCODE, @LF_PRODQTY, @LS_CUNITCODE, @LD_NOWDATE, @LS_WORKER)
	END
	
	-- 4. 작업장별 생산실적 등록

	-- 일자별 생산 SEQ
   DECLARE @LI_PRODSEQ INT
	SELECT @LI_PRODSEQ = ISNULL(MAX(PRODSEQ), 0) + 1
	  FROM TP_WorkcenterPerProd WITH(NOLOCK)
	 WHERE PLANTCODE	  = @PLANTCODE
	   AND WORKCENTERCODE = @WORKCENTERCODE
	   AND PRODDATE		  = @LS_NOWDATE

	INSERT INTO TP_WorkcenterPerProd (PLANTCODE,	 PRODSEQ,  WORKCENTERCODE,	  PRODDATE,	 ITEMCODE,	   ORDERNO,	 PRODQTY,  BADQTY,	   TOTALQTY,  UNITCODE,	  INLOTNO,	   LOTNO,	 MAKEDATE,	    MAKER)
							 VALUES (@PLANTCODE, @LI_PRODSEQ, @WORKCENTERCODE, @LS_NOWDATE, @ITEMCODE, @LS_ORDERNO, @PRODQTY, @BADQTY, @LF_TOTALQTY, @UNITCODE, @MATLOTNO, @LS_LOTNO, @LD_NOWDATE, @LS_WORKER)

	-- 5. 재공재고 차감 및 삭제
	IF (@LF_STOCKQTY - @LF_FINALQTY = 0)
	BEGIN
		DELETE TB_StockWIP
		 WHERE PLANTCODE	  = @PLANTCODE
		   AND WORKCENTERCODE = @WORKCENTERCODE
		   AND LOTNO		  = @MATLOTNO
	END

	UPDATE TB_StockWIP
	   SET STOCKQTY = STOCKQTY - @LF_FINALQTY
		  ,EDITDATE = @LD_NOWDATE
		  ,EDITOR	= @LS_WORKER
	 WHERE PLANTCODE	  = @PLANTCODE
	   AND WORKCENTERCODE = @WORKCENTERCODE
	   AND LOTNO		  = @MATLOTNO

	-- 6. 재공재고 차감이력 등록

	-- 일자별 SEQ
   DECLARE @LI_WIPSEQ INT
	SELECT @LI_WIPSEQ = ISNULL(MAX(INOUTSEQ), 0) + 1
	  FROM TB_StockWIPrec WITH(NOLOCK)
	 WHERE PLANTCODE	  = @PLANTCODE
	   AND RECDATE		  = @LS_NOWDATE

	INSERT INTO TB_StockWIPrec (PLANTCODE,	 INOUTSEQ,	   RECDATE,		LOTNO,		ITEMCODE,  WORKCENTERCODE, INOUTFLAG, INOUTCODE,   INOUTQTY,	  UNITCODE,	   MAKEDATE,	  MAKER)
					   VALUES (@PLANTCODE, @LI_WIPSEQ, @LS_NOWDATE, @MATLOTNO, @LS_CITEMCODE, @WORKCENTERCODE,	  'O',		'40',  @LF_FINALQTY, @LS_CUNITCODE, @LD_NOWDATE, @LS_WORKER)

	IF (@PRODQTY > 0) -- 양품이 있을 경우에 ..
	BEGIN
		-- 7. 공정재고 등록
		INSERT INTO TB_StockPP (PLANTCODE,	   LOTNO,  ITEMCODE, WHCODE,  STOCKQTY,		 INDATE,	MAKEDATE,	   MAKER)
					   VALUES (@PLANTCODE, @LS_LOTNO, @ITEMCODE, 'WH003', @PRODQTY, @LS_NOWDATE, @LD_NOWDATE, @LS_WORKER)
	
		-- 8. 공정재고 입고이력 등록
	
		-- 일자별 SEQ
	   DECLARE @LI_PPSEQ INT
		SELECT @LI_PPSEQ = ISNULL(MAX(INOUTSEQ), 0) + 1
		  FROM TB_StockPPrec WITH(NOLOCK)
		 WHERE PLANTCODE = @PLANTCODE
		   AND RECDATE	 = @LS_NOWDATE
	
		INSERT INTO TB_StockPPrec (PLANTCODE,  INOUTSEQ,	 RECDATE,	  LOTNO,  ITEMCODE, WHCODE, INOUTFLAG, INOUTCODE, INOUTQTY,  UNITCODE,	  MAKEDATE,		 MAKER)
						  VALUES (@PLANTCODE, @LI_PPSEQ, @LS_NOWDATE, @LS_LOTNO, @ITEMCODE, 'WH003',	'I',	 '45',	  @PRODQTY, @UNITCODE, @LD_NOWDATE, @LS_WORKER)
	END

	IF (@BADQTY > 0) -- 불량품이 있을 경우에 ..
	BEGIN
		-- 불량판정 순번(@LI_FLSEQ) & 불량판정번호(@LS_FAULTNO) 채번
	   DECLARE @LS_FAULTNO VARCHAR(30)
			  ,@LI_FLSEQ INT
		SELECT @LI_FLSEQ = ISNULL(MAX(INOUTSEQ), 0) + 1
		  FROM TB_FaultRec WITH(NOLOCK)
		 WHERE PLANTCODE = @PLANTCODE
		   AND INOUTDATE = @LS_NOWDATE
		SET @LS_FAULTNO = 'FL' + REPLACE((@LS_NOWDATE), '-', '') + RIGHT(('0000' + CONVERT(VARCHAR, @LI_FLSEQ)), 4)
		-- 9. 불량 재고 등록 (탈가람)
		INSERT INTO TB_Fault (PLANTCODE,  WORKCENTERCODE,  ITEMCODE,	  INDATE, FAULTQTY,	    FAULTNO,  UNITCODE,	   MAKEDATE,	  MAKER, CLOTNO)
					 VALUES (@PLANTCODE, @WORKCENTERCODE, @ITEMCODE, @LS_NOWDATE,  @BADQTY, @LS_FAULTNO, @UNITCODE, @LD_NOWDATE, @LS_WORKER, @MATLOTNO)
		SELECT * FROM TB_Fault
		-- 10. 불량재고 이력 등록 (탈가람)
		INSERT INTO TB_FaultRec (PLANTCODE,	 WORKCENTERCODE,  INOUTSEQ,	  INOUTDATE,     FAULTNO,  ITEMCODE, WHCODE, INOUTFLAG, INOUTCODE, QTY,   UNITCODE,	MAKEDATE,      MAKER,     ORDERNO)
						 VALUES(@PLANTCODE, @WORKCENTERCODE, @LI_FLSEQ, @LS_NOWDATE, @LS_FAULTNO, @ITEMCODE, 'WH009',   'I',       '70',    @BADQTY,  @UNITCODE, @LD_NOWDATE, @LS_WORKER, @LS_ORDERNO)
	END

	SELECT * FROM TB_FAULTRec
	SET @RS_CODE = 'S';
	SET @RS_MSG  = ISNULL(@LS_LOTNO, '');
END

```