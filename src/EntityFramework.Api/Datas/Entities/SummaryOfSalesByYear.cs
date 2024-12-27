using System;
using System.Collections.Generic;

namespace EntityFramework.Api.Datas.Entities;
public partial class SummaryOfSalesByYear
{
    public DateTime? ShippedDate { get; set; }

    public int OrderId { get; set; }

    public decimal? Subtotal { get; set; }
}
