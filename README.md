# mssql-generate-poco
mvc and web api with angularjs to generate poco of table and sp

<h2>PrePare</h2>
<ul>
  <li>Visual Studio 2013 up</li>
  <li>.net framework 4.6.1</li>
</ul>

<h2>What is this project can help you?</h2>
<p>If you have many dto for sp or table and this project is right for you.</p>
<p>This project can generate sp input and result params.</p>
<p>Of course focus on table of poco and auto generate stored procedure insert and update.</p>

<h2>Other solution</h2>
<p>Default is direct column name.</p>
<p>But If you want to have camelcase params then you can new another class below</p>
In the Controllers>TableController
<br/>
```csharp
private ITableService service;
public TableController()
{
  service = new TableService();
}
```
change to below
<br/>

```csharp
private ITableService service;
public TableController()
{
  service = new TableForCsharpNameService();
}
```

<h2>Publish notice</h2>
If you publish to iis.
You might meet some error.
That is you need to configure connection string.
But file is not have permission.
You need to allow permission from File>connection.txt.

