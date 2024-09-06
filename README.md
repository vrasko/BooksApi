# BooksApi
Basic CRUD operations on Embedded Sqlite DB. Architecture is based on simplicity and max speed. 
Used Minimal Api, Dapper technology. Web Api methods are requested via theirs urls. Test project contains Test methods, which call api points.
MVC and Entity framework omitted due to their complexity and low speed for this task.
In the case real application and real DB (MsSQL,Postgresql, Oracle, Mysql...} should be more efficient to do some operations in database procedures or functions

How to test API points via testproject with debugging
1. Start debugging Unit Test
2. While on the first line of your test code or before calling your local web api project
3. Right click on your web api project and Debug > Start new instance
