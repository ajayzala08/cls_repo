﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WebApplication3
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class CompanyFormationdbEntities : DbContext
    {
        public CompanyFormationdbEntities()
            : base("name=CompanyFormationdbEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<cls_additionalinfo_tbl> cls_additionalinfo_tbl { get; set; }
        public virtual DbSet<cls_addressdetails_tbl> cls_addressdetails_tbl { get; set; }
        public virtual DbSet<cls_beneficialowner_tbl> cls_beneficialowner_tbl { get; set; }
        public virtual DbSet<cls_companyincorporation_tbl> cls_companyincorporation_tbl { get; set; }
        public virtual DbSet<cls_corporatesubscriber_tbl> cls_corporatesubscriber_tbl { get; set; }
        public virtual DbSet<cls_director_tbl> cls_director_tbl { get; set; }
        public virtual DbSet<cls_sharecapital_tbl> cls_sharecapital_tbl { get; set; }
        public virtual DbSet<cls_statusmst_tbl> cls_statusmst_tbl { get; set; }
        public virtual DbSet<cls_subscriber_tbl> cls_subscriber_tbl { get; set; }
        public virtual DbSet<cls_ticketmst_tbl> cls_ticketmst_tbl { get; set; }
        public virtual DbSet<cls_ticketreply_tbl> cls_ticketreply_tbl { get; set; }
        public virtual DbSet<cls_usermst_tbl> cls_usermst_tbl { get; set; }
        public virtual DbSet<cls_agree_tbl> cls_agree_tbl { get; set; }
        public virtual DbSet<cls_secretary_tbl> cls_secretary_tbl { get; set; }
    }
}
