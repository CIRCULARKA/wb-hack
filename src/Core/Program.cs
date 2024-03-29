using System;
using WBBasket.Core;

var vendorCode = "";

Console.WriteLine("Hello. Adding item to basket using vendor code...");
Console.Write("Enter your bearer API key: ");
var bearerKey = Console.ReadLine();

var basket = new Basket(bearerKey);
while (true)
{
    try
    {
        Console.Write("Enter vendor code: ");
        vendorCode = Console.ReadLine();
        var idRequest = new IDRequest(vendorCode, 16);
        var task = basket.AddAsync(idRequest);
        Console.WriteLine("Request sent. It may take some time as I iterating through all WB service instances...");
        await task;
        Console.WriteLine("Request successfull. You can check out your WB basket to ensure everything works!");
    }
    catch (Exception e)
    {
        Console.Write("ERROR: " + e.Message + "\nEnter vendor code: ");
    }
}
