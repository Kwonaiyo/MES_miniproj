```SQL
USE [KDTB03_MES_2]
GO
/****** Object:  StoredProcedure [dbo].[TGR_QM_Fault_I1]    Script Date: 2023-08-30 오전 11:41:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		정원영
-- Create date: 2023-06-26
-- Description:	재검사 후 양품/불량수량 등록
-- =============================================
ALTER PROCEDURE [dbo].[TGR_QM_Fault_I1]
	 @PLANTCODE	   VARCHAR(10)	-- 공장
	,@FAULTNO	   VARCHAR(30)	-- 불량판정번호
	,@PRODQTY	   FLOAT		-- 양품수량
	,@FAULTQTY	   FLOAT		-- 불량수량
	,@PRODREASON   VARCHAR(200)	-- 양품사유
	,@DELWHY	   VARCHAR(20)	-- 불량원인
	,@DELREASON	   VARCHAR(200) -- 불량사유
	,@MAKER		   VARCHAR(20)	-- 등록자
	,@DELQTY	   FLOAT		-- 폐기수량
	,@DELSEQ	   INT			-- 행의 길이
	,@CHK_LENGTH   INT			-- 현재 행의 수

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

	-- TB_Fault에서 변수 찾아오기
   DECLARE @LS_ITEMCODE		  VARCHAR(30) -- 품목
		  ,@LS_WORKCENTERCODE VARCHAR(30) -- 작업장
		  ,@LS_UNITCODE		  VARCHAR(10) -- 단위
	SELECT @LS_ITEMCODE		  = ITEMCODE
		  ,@LS_WORKCENTERCODE = WORKCENTERCODE
		  ,@LS_UNITCODE		  = UNITCODE
	  FROM TB_Fault WITH(NOLOCK)
	 WHERE PLANTCODE = @PLANTCODE
	   AND FAULTNO   = @FAULTNO

	-- V 체크 뭐하지

   DECLARE @LS_ORDERNO VARCHAR(30)
	SELECT @LS_ORDERNO = ORDERNO
	  FROM TB_FaultRec WITH(NOLOCK)
	 WHERE PLANTCODE = @PLANTCODE
	   AND FAULTNO   = @FAULTNO

	-- 1. 양품수량이 있을 경우 로직
	IF (ISNULL(@PRODQTY, 0) > 0)
	BEGIN
		-- 1) TB_Fault 삭제
		DELETE TB_Fault
		 WHERE PLANTCODE      = @PLANTCODE
		   AND WORKCENTERCODE = @LS_WORKCENTERCODE
		   AND ITEMCODE       = @LS_ITEMCODE
		   AND FAULTNO	      = @FAULTNO

		-- TB_FaultRec 일자별 이력 SEQ
	   DECLARE @LI_PRODRECSEQ INT
		SELECT @LI_PRODRECSEQ = ISNULL(MAX(INOUTSEQ), 0) + 1
		  FROM TB_FaultRec WITH(NOLOCK)
		 WHERE PLANTCODE = @PLANTCODE
		   AND INOUTDATE = @LS_NOWDATE

	
		IF (ISNULL(@DELSEQ, 0) = @CHK_LENGTH) -- 마지막 행에서 한번만 시행
		BEGIN

		-- LOTNO 채번
		   DECLARE @LS_LOTNO  VARCHAR(30) -- 완제품 LOT NO
				  ,@LI_LOTSEQ INT		  -- LOT TRACKING의 공장별, LOTNO별 순번
			SELECT @LI_LOTSEQ = ISNULL(MAX(SEQ), 0) + 1
			  FROM TP_LotTracking WITH(NOLOCK)
			 WHERE PLANTCODE = @PLANTCODE
			   AND LOTNO	 = @LS_LOTNO
			SET @LS_LOTNO = DBO.FN_LOTNO('LT_FF_F')

		-- 2) TB_FaultRec 등록
		INSERT INTO TB_FaultRec (PLANTCODE,	    WORKCENTERCODE,		  INOUTSEQ,	  INOUTDATE,  FAULTNO,     ITEMCODE, WHCODE, INOUTFLAG, INOUTCODE, QTY,     UNITCODE,	  MAKEDATE, MAKER,     ORDERNO, NEWLOTNO)
						 VALUES(@PLANTCODE, @LS_WORKCENTERCODE, @LI_PRODRECSEQ, @LS_NOWDATE, @FAULTNO, @LS_ITEMCODE, 'WH009',   'O',       '75',   @PRODQTY, @LS_UNITCODE, @LD_NOWDATE, @MAKER, @LS_ORDERNO, @LS_LOTNO)

			
			
			-- 3) TB_StockPP 등록
			INSERT INTO TB_StockPP (PLANTCODE,     LOTNO,     ITEMCODE,      UNITCODE, WHCODE,  STOCKQTY,      INDATE,    MAKEDATE,  MAKER)
						   VALUES (@PlantCode, @LS_LOTNO, @LS_ITEMCODE,  @LS_UNITCODE, 'WH003', @PRODQTY, @LS_NOWDATE, @LD_NOWDATE, @MAKER)	

			-- 4) TB_StockPPRec 등록
		    -- 공정창고 입고이력 추가
		    -- 일자 별 입고 이력 SEQ 채번
		   DECLARE @PP_INOUTSEQ INT
			SELECT @PP_INOUTSEQ = ISNULL(MAX(INOUTSEQ),0) + 1
			  FROM TB_StockPPrec WITH(NOLOCK)
			 WHERE PLANTCODE = @PlantCode
			   AND RECDATE   = @LS_NOWDATE
		    
			INSERT INTO TB_StockPPrec(PLANTCODE,     INOUTSEQ,     RECDATE,     LOTNO,     ITEMCODE, WHCODE, INOUTFLAG, INOUTQTY,     UNITCODE, INOUTCODE, MAKEDATE,  MAKER)
							   VALUES(@PlantCode, @PP_INOUTSEQ, @LS_NOWDATE, @LS_LOTNO, @LS_ITEMCODE, 'WH003',   'I',    @PRODQTY, @LS_UNITCODE,   '75', @LD_NOWDATE, @MAKER)
		END

--		-- 상태테이블 변수정의???? 작업지시번호가 필요한데.. 이게 이전 생산실적등록할때 양품 된 LOT랑 연결해줘서 그 작업지시번호, 작업장, 공장, 품목, 단위, ->> 불량판정번호로 생산실적등록 양품LOT랑 연결시켜줄수도 있을듯? ( 불량판정번호가 애초에 생산실적등록될 때 만들어지기때문에..)
--		-- CLOTNO는 투입한 원자재의 LOTNO인데, 이거는 grid에서 체크된 품목들의 생산실적등록 시 양품 된 LOT별 원자재LOT를 연결시켜줘야한다. 
--		-- 임시불량품 만들어질 때 TB_FaultRec에 작업지시번호를 기록해놓음.
--	   DECLARE @LS_F_ORDERNO VARCHAR(30) -- 임시불량품에 대한 작업지시번호
--		SELECT @LS_F_ORDERNO = ORDERNO
--		  FROM TB_FaultRec WITH(NOLOCK)
--		 WHERE PLANTCODE	  = @PLANTCODE
--		   AND WORKCENTERCODE = @LS_WORKCENTERCODE
--		   AND FAULTNO        = @FAULTNO
--

	END

	-- 2. 불량수량이 있을 경우 로직
	IF (ISNULL(@FAULTQTY, 0) > 0)
	BEGIN
		-- 1) TB_Fault 삭제
		DELETE TB_Fault
		 WHERE PLANTCODE = @PLANTCODE
		   AND ITEMCODE  = @LS_ITEMCODE
		   AND FAULTNO	 = @FAULTNO

		-- TB_FaultRec 일자별 이력 SEQ
	   DECLARE @LI_FLRECSEQ INT
		SELECT @LI_FLRECSEQ = ISNULL(MAX(INOUTSEQ), 0) + 1
		  FROM TB_FaultRec WITH(NOLOCK)
		 WHERE PLANTCODE = @PLANTCODE
		   AND INOUTDATE = @LS_NOWDATE

		IF (ISNULL(@DELSEQ, 0) = @CHK_LENGTH) -- 마지막 행에서 한번만 시행
		BEGIN

		-- 2) TB_FaultRec 등록
			INSERT INTO TB_FaultRec (PLANTCODE,     WORKCENTERCODE,     INOUTSEQ,   INOUTDATE,	FAULTNO,     ITEMCODE, WHCODE, INOUTFLAG, INOUTCODE, QTY,      UNITCODE,    MAKEDATE,  MAKER)
							 VALUES(@PLANTCODE, @LS_WORKCENTERCODE, @LI_FLRECSEQ, @LS_NOWDATE, @FAULTNO, @LS_ITEMCODE, 'WH009',    'O',      '77',    @DELQTY,  @LS_UNITCODE, @LD_NOWDATE, @MAKER)
		
			-- 3) TB_Deleted 등록
			-- SEQ 채번
		   DECLARE @LI_DELSEQ INT
				  ,@LS_DELNO  VARCHAR(30)
			SELECT @LI_DELSEQ = ISNULL(MAX(DELSEQ), 0) + 1
			  FROM TB_Deleted WITH(NOLOCK)
			 WHERE PLANTCODE = @PLANTCODE
			   AND DELDATE = @LS_NOWDATE
			SET @LS_DELNO = 'DL' + REPLACE((@LS_NOWDATE), '-', '') + RIGHT(('0000' + CONVERT(VARCHAR, @LI_DELSEQ)), 4)

			INSERT INTO TB_Deleted (PLANTCODE,	   WORKCENTERCODE,     DELSEQ,     DELDATE,     DELNO,     ITEMCODE, WHCODE, INOUTFLAG, INOUTCODE,  DELQTY,     UNITCODE,  DELWHY,   DELREASON, WORKER,    MAKEDATE,  MAKER)
							VALUES(@PLANTCODE, @LS_WORKCENTERCODE, @LI_DELSEQ, @LS_NOWDATE, @LS_DELNO, @LS_ITEMCODE, 'WH010',   'I',       '77',    @DELQTY, @LS_UNITCODE, @DELWHY, @DELREASON, @MAKER, @LD_NOWDATE, @MAKER)
		END
	END
	SET @RS_CODE = 'S';
	SET @RS_MSG = ISNULL(@LS_LOTNO, '0');
END
```