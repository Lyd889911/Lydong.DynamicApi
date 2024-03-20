<a name="qI8Fi"></a>
# 介绍
Lydong.DynamicApi可以在应用层动态的生成接口。不用再写Controller了。
<a name="P7zXm"></a>
# 使用
```csharp
builder.Services.AddControllers();

builder.Services.AddDynamicApi();
```
AddDynamicApi可以传递`DynamicApiOption`配置参数进入，不传参数就是默认参数。
```csharp
HttpVerbs = new()
{
    ["add"] = "POST",
    ["create"] = "POST",
    ["post"] = "POST",

    ["get"] = "GET",
    ["find"] = "GET",
    ["fetch"] = "GET",
    ["query"] = "GET",

    ["update"] = "PUT",
    ["put"] = "PUT",
    ["modify"] = "PUT",

    ["delete"] = "DELETE",
    ["remove"] = "DELETE",
};
```
默认动词映射以上。方法名前面出现key。会自动去掉，并选择value的http方法。可以在配置的时候自己添加一些合适自己的配置。<br />其他的配置看`DynamicApiOption`属性，有注释。

实现`IDynamicApi`接口的类，里面的public方法会被暴露出去。
```csharp
public class TestService: IDynamicApi
```
不想被暴露又是public的方法，标记`[NonDynamicApi]`特性。
```csharp
public class TestService: IDynamicApi
{

    //GET /api/test/t1
    public string GetT1()
    {
        return "t1";
    }


    //POST /api/test/t2
    [HttpPost]
    public string GetT2()
    {
        return "t2";
    }

    //GET /api/test/t3 query
    public string GetT3(TestParam p)
    {
        return $"t3/{p.Id}/{p.Name}/{p.Age}";
    }

    //POST /api/test/t4 body
    [HttpPost]
    public string T4(TestParam p)
    {
        return $"t4/{p.Id}/{p.Name}/{p.Age}";
    }


    //PUT /api/test/t5 body
    [HttpPut]
    public string T5(TestParam p)
    {
        return $"t5/{p.Id}/{p.Name}/{p.Age}";
    }


    //DELETE /api/test/t6 body
    [HttpDelete]
    public string T6(TestParam p)
    {
        return $"t6/{p.Id}/{p.Name}/{p.Age}";
    }

    //GET /api/test/t7 route
    [Route("{id}")]
    public string GetT7([FromRoute]string id)
    {
        return $"t7/{id}";
    }

    //不暴露
    [NonDynamicApi]
    public string GetT8()
    {
        return "t8";
    }

    //不暴露
    private string GetT9()
    {
        return "t9";
    }
}
```
