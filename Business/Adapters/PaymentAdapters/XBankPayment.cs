using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Business.Abstract;
using Castle.DynamicProxy.Generators.Emitters;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using Core.Utilities.Results;
using Entities.Concrete;

namespace Business.Adapters.PaymentAdapters
{
    public class XBankPayment:IBankPosService
    {
        public IResult Pay(CreditCardExtend creditCard, double amount)
        {
            //Şifrelenmiş olarak iletilir
            XBankFaraziPaymentSystem xBankFaraziPaymentSystem = new XBankFaraziPaymentSystem();
            var result = xBankFaraziPaymentSystem.PaymentSystem(
                creditCard.CardNumber,
                creditCard.CardHolder,
                creditCard.ExpYear,
                creditCard.ExpMonth,
                creditCard.Cvv, amount);
            if (result["success"] == "success")
            {
                return new SuccessResult(result["message"]);
            }
            else
            {
                return new ErrorResult(result["message"]);
            }
        }
    }

    //Banka apisinde yer alan ödeme sistemi simülasyonu - SMS gönderimi eksik :)
    public class XBankFaraziPaymentSystem
    {
        //Örnek kartlar
        List<XBankCard> _cards = new List<XBankCard>()
        {
                new XBankCard() {CardId = 1, CardNumber = "0000000000000000", CardHolder = "JOHN SMITH", ExpirationYear = "2020", ExpirationMonth = "11", Cvv = "771", AvailableLimit = 512.12},
                new XBankCard() {CardId = 2, CardNumber = "0000000000000000", CardHolder = "JANE DOE", ExpirationYear = "2021", ExpirationMonth = "02", Cvv = "423", AvailableLimit = 1675.00},
                new XBankCard() {CardId = 3, CardNumber = "0000000000000000", CardHolder = "MICHAEL JAEL", ExpirationYear = "2021", ExpirationMonth = "04", Cvv = "268", AvailableLimit = 33.42},
                new XBankCard() {CardId = 4, CardNumber = "0000000000000000", CardHolder = "LINDA MINA", ExpirationYear = "2023", ExpirationMonth = "08", Cvv = "804", AvailableLimit = 45476.89},
                new XBankCard() {CardId = 5, CardNumber = "0000000000000000", CardHolder = "GEORGE BORCH", ExpirationYear = "2022", ExpirationMonth = "01", Cvv = "130", AvailableLimit = 0.55}
        };

        public Dictionary<string, string> PaymentSystem(string cardNumber, string cardHolder, string expirationYear,
            string expirationMonth,
            string cvv, double amount)
        {
            Dictionary<string, string> _result = new Dictionary<string, string>();

            bool dateValidation = true;
            if (Convert.ToInt32(expirationYear) < DateTime.Now.Year)
            {
                dateValidation = false;
            }
            else
            {
                if ((Convert.ToInt32(expirationMonth) < DateTime.Now.Month) &&
                    (Convert.ToInt32(expirationYear) == DateTime.Now.Year))
                {
                    dateValidation = false;
                }
            }

            if (!dateValidation)
            {
                _result.Add("succes","error");
                _result.Add("message", "Credit card expired");
                return _result;
            }

            var cardResult = _cards.Find(card => 
                card.CardNumber == cardNumber 
                && card.CardHolder == cardHolder
                && card.ExpirationYear == expirationYear
                && card.ExpirationMonth == expirationMonth
                && card.Cvv == cvv
                );
            if (cardResult == null )
            {
                _result.Add("succes", "error");
                _result.Add("message", "Credit card information doesn't match system records.");
            }
            else
            {
                if (cardResult.AvailableLimit < amount)
                {
                    _result.Add("succes", "error");
                    _result.Add("message", "The balance is insufficient.");
                }
                else
                {
                    cardResult.AvailableLimit -= amount;
                    _result.Add("succes", "succes");
                    _result.Add("message", "Payment transaction has been completed.");
                }
            }
            return _result;
        }
    }

    //Banka apisinde yer alan kart bilgileri sınıfı
    public class XBankCard
    {
        public int CardId { get; set; }
        public string CardHolder { get; set; }
        public string CardNumber { get; set; }
        public string ExpirationYear { get; set; }
        public string ExpirationMonth { get; set; }
        public string Cvv { get; set; }
        public double AvailableLimit { get; set; }
    }
}
