﻿using Business.Abstract;
using Business.Constants;
using Business.ValidationRules.FluentValidation;
using Core.Aspects.Autofac.Caching;
using Core.Aspects.Autofac.Performance;
using Core.Aspects.Autofac.Validation;
using Core.CrossCuttingConcerns.Validation.FluentValidation;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Adapters.PaymentAdapters;
using Core.Aspects.Autofac.Transaction;

namespace Business.Concrete
{
    public class RentalManager : IRentalService
    {
        IRentalDal _rentalDal;
        IBankPosService _bankPosService;
        ICreditCardService _creditCardManager;


        public RentalManager(IRentalDal rentalDal, IBankPosService bankPosService, ICreditCardService creditCardManager)
        {
            _rentalDal = rentalDal;
            _bankPosService = bankPosService;
            _creditCardManager = creditCardManager;
        }

        [ValidationAspect(typeof(RentalValidator))]
        [CacheRemoveAspect("IRentalService.Get")]
        [CacheRemoveAspect("ICarService.Get")]
        public IResult Add(Rental rental)
        {
            //if (CarRentedControl(rental.CarId).Success == false)
            //{
            //    return new ErrorResult(Messages.ExistCarRental);
            //}

            //var result = ValidationTool.Validate(new RentalValidator(_rentalDal), rental);
            //if (!result.Success)
            //{
            //    return new ErrorResult(Messages.ExistCarRental);
            //}

            IResult result = IsRentableCar(rental);
            if (!result.Success)
            {
                return new ErrorResult(Messages.ExistCarRental);
            }

            _rentalDal.Add(rental);
            return new SuccessResult();
        }

        [ValidationAspect(typeof(RentalValidator))]
        [ValidationAspect(typeof(CreditCardExtendValidator))]
        [TransactionScopeAspect]
        public IResult RentalOrder(RentalOrder rentalOrder)
        {
            IResult result = Add(rentalOrder.RentalContent);
            if (!result.Success)
            {
                return new ErrorResult(result.Message);
            }

            if (rentalOrder.IsSave)
            {
                CreditCard newCard = new CreditCard()
                {
                    CardName = rentalOrder.PayCard.CardName,
                    CardHolder = rentalOrder.PayCard.CardHolder,
                    CardNumber = rentalOrder.PayCard.CardNumber,
                    CustomerId = rentalOrder.PayCard.CustomerId,
                    ExpYear = rentalOrder.PayCard.ExpYear,
                    ExpMonth = rentalOrder.PayCard.ExpMonth
                };
                result = _creditCardManager.Add(newCard);
                if (!result.Success)
                {
                    return new ErrorResult(result.Message);
                }
            }

            result = _bankPosService.Pay(rentalOrder.PayCard, rentalOrder.Amount);
            if (!result.Success)
            {
                return new ErrorResult(result.Message);
            }

            return new SuccessResult(Messages.RentAndPay);
        }

        //public IResult IsRentableCar(int carId)
        //{
        //    var result = _rentalDal.GetAll(r => r.CarId == carId && r.ReturnDate == null);
        //    if(result != null && result.Count > 0)
        //    {
        //        return new ErrorResult();
        //    }
        //    return new SuccessResult();
        //}

        [CacheRemoveAspect("IRentalService.Get")]
        [CacheRemoveAspect("ICarService.Get")]
        public IResult Delete(Rental rental)
        {
            _rentalDal.Delete(rental);
            return new SuccessResult();
        }

        [CacheAspect]
        [PerformanceAspect(5)]
        public IDataResult<List<Rental>> GetAll()
        {
            return new SuccessDataResult<List<Rental>>(_rentalDal.GetAll());
        }

        [CacheAspect]
        public IDataResult<Rental> GetById(int id)
        {
            return new SuccessDataResult<Rental>(_rentalDal.Get(r => r.Id == id));
        }

        [CacheAspect]
        [PerformanceAspect(10)]
        public IDataResult<List<RentalDetailDto>> GetAllRentalDetails()
        {
            return new SuccessDataResult<List<RentalDetailDto>>(_rentalDal.GetAllDetails());
        }

        [CacheAspect]
        public IDataResult<List<RentalDetailDto>> GetRentalDetailsByCarId(int carId)
        {
            return new SuccessDataResult<List<RentalDetailDto>>(_rentalDal.GetAllDetailsBy(r => r.CarId == carId));
        }

        [CacheAspect]
        public IDataResult<List<RentalDetailDto>> GetRentalDetailsByCustomerId(int customerId)
        {
            return new SuccessDataResult<List<RentalDetailDto>>(_rentalDal.GetAllDetailsBy(r => r.CustomerId == customerId));
        }

        [CacheAspect]
        public IDataResult<List<Rental>> GetRentalsByCarId(int carId)
        {
            return new SuccessDataResult<List<Rental>>(_rentalDal.GetAll(r => r.CarId == carId));
        }

        [CacheAspect]
        public IDataResult<List<Rental>> GetRentalsByCustomerId(int customerId)
        {
            return new SuccessDataResult<List<Rental>>(_rentalDal.GetAll(r => r.CustomerId == customerId));
        }

        [CacheRemoveAspect("IRentalService.Get")]
        [CacheRemoveAspect("ICarService.Get")]
        public IResult Update(Rental rental)
        {
            IResult result = IsRentableCar(rental, true);
            if (!result.Success)
            {
                return new ErrorResult(Messages.ExistCarRental);
            }

            _rentalDal.Update(rental);
            return new SuccessResult();
        }

        [CacheAspect]
        public IDataResult<RentalDetailDto> GetRentalDetailsById(int id)
        {
            return new SuccessDataResult<RentalDetailDto>(_rentalDal.GetDetails(r => r.Id == id));
        }

        public IResult IsRentableCar(Rental rental, bool isUpdate = false)
        {
            List<Rental> rentalDetail = (isUpdate)
                ? _rentalDal.GetAll(r => r.CarId == rental.CarId && r.ReturnDate == null && r.Id != rental.Id)
                : _rentalDal.GetAll(r => r.CarId == rental.CarId && r.ReturnDate == null);
            if (rentalDetail != null && rentalDetail.Count > 0)
            {
                rentalDetail = rentalDetail.Where(r =>
                    (r.RentStartDate <= rental.RentStartDate && r.RentEndDate >= rental.RentStartDate) ||
                    r.RentStartDate <= rental.RentEndDate && r.RentEndDate >= rental.RentEndDate).ToList();
                if (rentalDetail != null && rentalDetail.Count > 0)
                {
                    return new ErrorResult();
                }
            }
            return new SuccessResult();
        }
    }
}
