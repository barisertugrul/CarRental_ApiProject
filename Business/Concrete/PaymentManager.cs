﻿using System;
using System.Collections.Generic;
using System.Text;
using Business.Abstract;
using Business.Constants;
using Business.ValidationRules.FluentValidation;
using Core.Aspects.Autofac.Transaction;
using Core.Aspects.Autofac.Validation;
using Core.Utilities.Results;
using Entities.Concrete;

namespace Business.Concrete
{
    public class PaymentManager : IPaymentService
    {
        IRentalService _rentalService;
        ICreditCardService _creditCardService;
        IBankPosService _bankPosService;

        public PaymentManager(ICreditCardService creditCardService, IRentalService rentalService, IBankPosService bankPosService)
        {
            _creditCardService = creditCardService;
            _rentalService = rentalService;
            _bankPosService = bankPosService;
        }

        [ValidationAspect(typeof(CreditCardExtendValidator))]
        [TransactionScopeAspect]
        public IResult Pay(Payment payment)
        {

            IDataResult<Rental> rentalResult = _rentalService.GetById(payment.RentalId);
            if (!rentalResult.Success)
            {
                return new ErrorResult(rentalResult.Message);
            }

            var result = _bankPosService.Pay(payment.PayCard, rentalResult.Data.Amount);
            if (!result.Success)
            {
                return new ErrorResult(result.Message);
            }

            rentalResult.Data.PayConfirm = true;
            result = _rentalService.Update(rentalResult.Data);
            if (!result.Success)
            {
                return new ErrorResult(result.Message);
            }

            if (payment.IsSave)
            {
                CreditCard newCard = new CreditCard()
                {
                    CardName = payment.PayCard.CardName,
                    CardHolder = payment.PayCard.CardHolder,
                    CardNumber = payment.PayCard.CardNumber,
                    CustomerId = payment.PayCard.CustomerId,
                    ExpYear = payment.PayCard.ExpYear,
                    ExpMonth = payment.PayCard.ExpMonth
                };
                result = _creditCardService.Add(newCard);
                if (!result.Success)
                {
                    return new ErrorResult(Messages.PayButNotSave);
                }
            }
            return new SuccessResult(Messages.PaymentComplete);
        }
    }
}
