# 自定义EF Core 的全局筛选，软删除以及公共字段

## 实现创建和更新时间自动更新

我们可以通过重载`SaveChangesAsync`方法，在提交数据时，对创建时间和更新时间进行设置。

```csharp
public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
{
    var entries = ChangeTracker.Entries().Where(e => e.State == EntityState.Added).ToList();
    foreach (var entityEntry in entries)
    {
        var property = entityEntry.Metadata.FindProperty("CreatedTime");
        if (property != null && property.ClrType == typeof(DateTimeOffset))
        {
            entityEntry.Property("CreatedTime").CurrentValue = DateTimeOffset.UtcNow;
        }
    }
    entries = ChangeTracker.Entries().Where(e => e.State == EntityState.Modified).ToList();
    foreach (var entityEntry in entries)
    {
        var property = entityEntry.Metadata.FindProperty("UpdatedTime");
        if (property != null && property.ClrType == typeof(DateTimeOffset))
        {
            entityEntry.Property("UpdatedTime").CurrentValue = DateTimeOffset.UtcNow;

        }
    }
    return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
}

```

以上代码，是将所有模型中存在`CreatedTime`或`UpdatedTime`的字段进行更新，请根据实际需求进行更改。

## 实现模型主键设置和软删除

我们定义了一个接口，所有实体继承该接口，该接口中有`Id`和`IsDeleted`字段，用来标识主键和是否删除(软删除)。

```csharp
private void OnModelExtendCreating(ModelBuilder modelBuilder)
{
    var entityTypes = modelBuilder.Model.GetEntityTypes();
    foreach (var entityType in entityTypes)
    {
        if (typeof(IEntityBase).IsAssignableFrom(entityType.ClrType))
        {
            modelBuilder.Entity(entityType.Name)
                .HasKey("Id");
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(ConvertFilterExpression<IEntityBase>(e => !e.IsDeleted, entityType.ClrType));
        }
    }
}

private static LambdaExpression ConvertFilterExpression<TInterface>(Expression<Func<TInterface, bool>> filterExpression, Type entityType)
{
    var newParam = Expression.Parameter(entityType);
    var newBody = ReplacingExpressionVisitor.Replace(filterExpression.Parameters.Single(), newParam, filterExpression.Body);

    return Expression.Lambda(newBody, newParam);
}
```

以上代码，我们先判断了实体是否是实现了接口，如果实现了，那么就可以设置主键和软删除。
