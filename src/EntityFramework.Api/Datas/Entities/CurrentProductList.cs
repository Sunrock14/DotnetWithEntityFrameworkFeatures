using System;
using System.Collections.Generic;

namespace EntityFramework.Api.Datas.Entities;
public partial class CurrentProductList
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;
}
