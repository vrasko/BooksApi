# BooksApi
Basic CRUD operations on Embedded Sqlite DB. Architecture is based on simplicity and max speed. There could be many solutions to achieve the goal.

The Api point Insert Book is built as a more complex example. The object model Book and Author are entered to Api. The assumption is that book is given by its EAN barcode. Api function retrieve author, if he exists, returns his ID, if don't insert new author record. After that book is retrieved. If found number of prints is incremented, if doesn't new book record is added.Ther could be more options to search records by. It depends on the requirements...

There is used Minimal Api, Dapper ORM mapper for communication with DB. Web Api methods are requested via theirs urls. Test project contains Test methods, which call api points. For convenient developing process and tests SQlite was used  as a DB data storage. 
MVC and Entity framework omitted due to their complexity and low speed for this task.
In the case real application and real DB (MsSQL,Postgresql, Oracle, Mysql...} should be more efficient to do some operations in database procedures or functions directly.

For testing manipulate with Embedded database Sqlite with e.g. DBeaver - universal database manager

Localization of log file and connection string are in appsettings.json. Modify them if needed.

How to test API points via testproject with debugging
1. Start debugging Unit Test
2. While on the first line of your test code or before calling your local web api project
3. Right click on your web api project and Debug > Start new instance


Testing of e-mail client and e-mail sending was done via MailTrap sandbox. Smtp client configuration is in appsettings.json.


