using OnlineAuctionProject.Configurations;
using PayPal.Api;
using System;
using System.Collections.Generic;

namespace OnlineAuctionProject.Services
{
    public static class PayPalPaymentService
    {
        public static Payment CreatePayment(string baseUrl, Models.Payment paymentData)
        {
            var apiContext = PayPalConfiguration.GetAPIContext();
            var payment = new Payment()
            {
                intent = "sale",
                payer = new Payer() { payment_method = "paypal" },
                transactions = GetTransactionsList(paymentData),
                redirect_urls = GetReturnUrls(baseUrl)
            };

            var createdPayment = payment.Create(apiContext);

            return createdPayment;
        }

        private static List<Transaction> GetTransactionsList(Models.Payment paymentData)
        {
            var transactionList = new List<Transaction>();
            var total = paymentData.PaymentValue;
            var tax = (double.Parse(paymentData.PaymentValue) * 0.1).ToString();
            var shipping = (double.Parse(paymentData.PaymentValue) * 0.1).ToString();
            var subtotal = (double.Parse(paymentData.PaymentValue) * 0.8).ToString();
            transactionList.Add(new Transaction()
            {
                description = "Transaction description.",
                invoice_number = Guid.NewGuid().ToString(),
                amount = new Amount()
                {
                    currency = "USD",
                    total = total,
                    details = new Details()
                    {
                        tax = tax,
                        shipping = shipping,
                        subtotal = subtotal
                    }
                },
                item_list = new ItemList()
                {
                    items = new List<Item>()
                    {
                        new Item()
                        {
                            name = "Your winning lot",
                            currency = "USD",
                            price = subtotal,
                            quantity = "1",
                            sku = "sku"
                        }
                    }
                }
            });
            return transactionList;
        }

        private static RedirectUrls GetReturnUrls(string baseUrl)
        {
            return new RedirectUrls()
            {
                cancel_url = baseUrl,
                return_url = baseUrl,
            };
        }

        public static Payment ExecutePayment(string paymentId, string payerId)
        {
            var apiContext = PayPalConfiguration.GetAPIContext();
            var paymentExecution = new PaymentExecution() { payer_id = payerId };
            var payment = new Payment() { id = paymentId };
            var executedPayment = payment.Execute(apiContext, paymentExecution);

            return executedPayment;
        }
    }
}