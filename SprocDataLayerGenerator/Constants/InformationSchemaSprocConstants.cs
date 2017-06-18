using System;
using System.Collections.Generic;
using System.Text;

namespace SprocDataLayerGenerator
{

    public static class MetaSprocDependencyConstants
    {
        public const string GET_DEPENDENCY = @"
select o2.Name as 'ReferencedObject',
		o2.type as 'ReferencedType',
		o.Name as 'ReferencingObject',
		d.*
from sys.objects o
inner join sys.sql_dependencies d 
on o.object_id = d.object_id
inner join sys.objects o2
on o2.object_id = d.referenced_major_id 

where o.type in('P','U')
and (o.Name = @SprocName or @SprocName is null)";

        public const string GET_DEPENDENCIES = @"
if @SprocName is not null
	begin
		exec GetDependency @SprocName
	end
if @SprocName is null
begin
        exec GetDependency
end";
    }
    public static class InformationSchemaSprocConstants
    {
        #region information schema sprocs public constants

        public const string GET_INFORMATION_SCHEMA_TABLES = @"

  SELECT t.*
	from Information_Schema.Tables t
where (t.Table_Name = @TableName or @TableName is null)
and t.Table_Name <> 'dtproperties'
and t.Table_Name <> 'sysdiagrams'

";
        public const string GET_INFORMATION_SCHEMA_COLUMNS = @"

  
	SELECT c.*
	FROM Information_Schema.Tables t
	inner join Information_Schema.Columns c
	on c.Table_Name = t.Table_Name
	--WHERE t.TABLE_TYPE IN ('BASE TABLE', 'VIEW') 
	WHERE (t.Table_Name = @TableName or @TableName is null)
	and t.TABLE_TYPE IN ('BASE TABLE') 
	and t.Table_Name <> 'dtproperties'
    and t.Table_Name <> 'sysdiagrams'

	ORDER BY t.TABLE_NAME

";
        public const string GET_INFORMATION_SCHEMA_TABLE_CONSTRAINTS = @"


   select tc.*
	from Information_Schema.Tables t
	inner join Information_Schema.Table_Constraints tc
	on t.Table_Name = tc.Table_Name
	where (t.Table_Name = @TableName or @TableName is null)
	and t.Table_Type IN ('BASE TABLE') 
	and t.Table_Name <> 'dtproperties'
    and t.Table_Name <> 'sysdiagrams'

	Order by t.Table_Name	

";
        public const string GET_INFORMATION_SCHEMA = @"

if (@TableName is null)
	begin
		Exec GetInformationSchemaTables
		Exec GetInformationSchemaTableConstraints
		Exec GetInformationSchemaColumns
		Exec GetInformationSchemaColumnUsage
   end

if(@TableName is not null)
	begin
		Exec GetInformationSchemaTables @TableName
		Exec GetInformationSchemaTableConstraints @TableName
		Exec GetInformationSchemaColumns @TableName
		Exec GetInformationSchemaColumnUsage @TableName
    end
	

";

        public const string GET_INFORMATION_SCHEMA_CONSTRAINT_COLUMN_USAGE = @"select ccu.*
from Information_Schema.Tables t
inner join Information_Schema.Constraint_Column_Usage ccu
on ccu.Table_Name = t.Table_Name
where (t.Table_Name = @TableName or @TableName is null)
	and t.Table_Type IN ('BASE TABLE') 
	and t.Table_Name <> 'dtproperties'
    and t.Table_Name <> 'sysdiagrams'

";

        public const string IS_IDENTITY_COLUMN = @"if exists(select *
		from information_schema.columns 
		where 
		table_schema = 'dbo' 
		and columnproperty(object_id(@TableName), @ColumnName,'IsIdentity') = 1 
		)
			set @IsIdentity = 1
else
			set @IsIdentity = 0";

        #endregion

    }
}
