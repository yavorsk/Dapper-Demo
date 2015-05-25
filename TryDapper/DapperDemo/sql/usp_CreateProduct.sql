USE [Northwind]
GO

/****** Object:  StoredProcedure [dbo].[usp_CreateProduct]    Script Date: 26-May-15 00:00:36 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[usp_CreateProduct] @ProductID int OUTPUT, 
											@ProductName nvarchar(40),
											@SupplierID int,   
											@CategoryID int,
											@QuantityPerUnit nvarchar(20),
											@UnitPrice money,
											@UnitsInStock smallint,
											@UnitsOnOrder smallint,
											@ReorderLevel smallint,
											@Discontinued bit
AS
BEGIN
	INSERT INTO Products (ProductName, SupplierID, CategoryID, QuantityPerUnit, UnitPrice, UnitsInStock, UnitsOnOrder, ReorderLevel, Discontinued)
	VALUES (@ProductName, @SupplierID, @CategoryID, @QuantityPerUnit, @UnitPrice, @UnitsInStock, @UnitsOnOrder, @ReorderLevel, @Discontinued)

	SET @ProductID = SCOPE_IDENTITY()

	RETURN
END
GO


