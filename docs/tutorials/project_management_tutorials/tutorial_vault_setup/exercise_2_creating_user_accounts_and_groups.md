---
title: "Exercise 2: Creating User Accounts and Groups"
url: "https://help.autodesk.com/cloudhelp/2025/ENG/Civil3D-Tutorials/files/GUID-7877BE4E-5527-4BE1-ABF5-691BC662BF56.htm"
category: "tutorial_vault_setup"
last_updated: "2026-03-17T18:42:21.484Z"
---

                  Exercise 2: Creating User Accounts and Groups  

# Exercise 2: Creating User Accounts and Groups

In this exercise, you will create two user accounts and two user groups that can access files in the database.

This procedure assumes that you're already logged in to the database as described in [Exercise 1: Logging In to Autodesk Vault](GUID-F3D4C11B-6BF8-4A3F-92B3-EA7AD90EE5BC.htm "In this exercise, you will log in to Autodesk Vault to prepare for other project tasks.").

Note:

You cannot create a user group without assigning at least one user to the group. Therefore, you should create some user accounts before creating groups.

Create user accounts

1.  To open Autodesk Vault Explorer, in Toolspace, on the Prospector tab, right-click the Projects collection, and click Autodesk Vault.
2.  In the Welcome dialog box, click Log In.
3.  In the Log In dialog box, verify the Administrator’s login data, then click OK.
4.  In Autodesk Vault, ensure that the correct database (vault) is selected.
    
    If you are using the default stand-alone server, there is only one database, and it is Vault. The current server, database (Vault), and user are displayed in the lower right corner of Autodesk Vault
    
5.  In Autodesk Vault, click Tools menu ![](../images/ac.menuaro.gif)Administration![](../images/ac.menuaro.gif)Global Settings.
6.  In the Global Settings dialog box, on the Security tab, click Users.
7.  In the User Management dialog box, click New User.
8.  In the New User dialog box, enter the following information:
    *   First Name: **Pat**
    *   Last Name: **Red**
    *   User Name: **pred**
    *   Password: **red123**
    *   Confirm Password: **red123**
9.  Select Enable User. Click OK.
10.  Repeat steps 7 through 9 to create another user profile as follows:
     *   First Name: **Kim**
     *   Last Name: **Green**
     *   User Name: **kgreen**
     *   Password: **green123**
     *   Confirm Password: **green123**
11.  Close the User Management dialog box.

Create user groups

1.  In the Global Settings dialog box, on the Security tab, click Groups.
2.  In the Groups dialog box, click New Group.
3.  In the Group dialog box, in the Group Name field, enter **Engineers**.
4.  Click the Roles button, then select Administrator. Click OK.
5.  Click the Vaults button, then select the database that your test users will work in. Select the default value, which is Vault. Click OK.
6.  Select Enable Group.
7.  Click the Add button.
8.  To add the member pred to the group, in the Add Members dialog box, in the Available Members table, click the row for **pred**. Click Add.
9.  Click OK to close the Add Members dialog box.
10.  Click OK to close the Group dialog box.
11.  In the Group Management dialog box, click New Group again, and repeat Steps 3 through 10 to create a group named **Technicians** with the role of Document Consumer. This group will have access to the same Vault database. Add user **kgreen** to the Technicians group.
     
     You will not see the advantages of creating user groups in this tutorial, but the structure you have just created is recommended for project teams. Groups can have different roles in relation to a particular database, and you can change the roles of users by moving them into the appropriate group.
     
12.  Click Close to close the Global Settings dialog box and then close Autodesk Vault.

To continue this tutorial, go to [Exercise 3: Creating a Project](GUID-256C9E05-8485-4340-AB16-3550414EFC63.htm "This exercise demonstrates how to log in to the project management system and create a project.").

**Parent topic:** [Tutorial: Vault Setup](GUID-05E12EDB-B70D-4E41-9C06-A304036C9C74.htm "In this tutorial, you will act as a project administrator, creating a project in Autodesk Vault and some sample users.")