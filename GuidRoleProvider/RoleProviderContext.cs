﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace GuidRoleProvider
{
    internal sealed class RoleProviderContext : IDisposable
    {
        private SqlConnection sqlConn = new SqlConnection();
        public DataSet db = new DataSet("RoleProvider");
        private SqlDataAdapter userAdapter;
        private SqlDataAdapter roleAdapter;
        private SqlDataAdapter userRoleAdapter;

        public RoleProviderContext()
        {
            sqlConn.ConnectionString = ConfigurationManager.ConnectionStrings["RoleProviderContext"].ConnectionString;
            sqlConn.Open();

            userAdapter = new SqlDataAdapter("select * from Users", sqlConn);
            userAdapter.FillSchema(db, SchemaType.Source, "Users");
            userAdapter.Fill(db, "Users");

            roleAdapter = new SqlDataAdapter("select * from Roles", sqlConn);
            roleAdapter.FillSchema(db, SchemaType.Source, "Roles");
            roleAdapter.Fill(db, "Roles");

            userRoleAdapter = new SqlDataAdapter("select * from UserRoles", sqlConn);
            userRoleAdapter.FillSchema(db, SchemaType.Source, "UserRoles");
            userRoleAdapter.Fill(db, "UserRoles");

            db.Relations.Add("UserKey", db.Tables["Users"].Columns["UserId"], db.Tables["UserRoles"].Columns["UserId"]);
            db.Relations.Add("RoleKey", db.Tables["Roles"].Columns["RoleId"], db.Tables["UserRoles"].Columns["RoleId"]);

            using(SqlDataAdapter junctionAdapter = new SqlDataAdapter("select u.*, r.* from UserRoles ur join Users u on ur.UserId = u.UserId join Roles r on ur.RoleId = r.RoleId", sqlConn))
            {
                junctionAdapter.FillSchema(db, SchemaType.Source, "Junction");
                junctionAdapter.Fill(db, "Junction");
            }

            db.Relations.Add("UserJunction", db.Tables["Users"].Columns["UserId"], db.Tables["Junction"].Columns["UserId"]);
            db.Relations.Add("RoleJunction", db.Tables["Roles"].Columns["RoleId"], db.Tables["Junction"].Columns["RoleId"]);
        }

        public void SaveChanges()
        {
            userAdapter.UpdateCommand = new SqlCommandBuilder(userAdapter).GetUpdateCommand();
            userAdapter.Update(db, "Users");

            roleAdapter.UpdateCommand = new SqlCommandBuilder(roleAdapter).GetUpdateCommand();
            roleAdapter.Update(db, "Roles");

            userRoleAdapter.UpdateCommand = new SqlCommandBuilder(userRoleAdapter).GetUpdateCommand();
            userRoleAdapter.Update(db, "UserRoles");
        }

        public void Dispose()
        {
            db.Dispose();
            roleAdapter.Dispose();
            userAdapter.Dispose();
            userRoleAdapter.Dispose();
            sqlConn.Dispose();
        }
    }
}
