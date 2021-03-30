﻿using Core.Utilities.Results;
using Entities.Concrete;
using Entities.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Abstract
{
    public interface IRentalService
    {
        IDataResult<List<Rental>> GetAll();
        IDataResult<Rental> GetById(int id);
        IDataResult<List<Rental>> GetRentalsByCustomerId(int customerId);
        IDataResult<List<Rental>> GetRentalsByCarId(int carId);
        IDataResult<List<RentalDetailDto>> GetAllRentalDetails();
        IDataResult<RentalDetailDto> GetRentalDetailsById(int id);
        IDataResult<List<RentalDetailDto>> GetRentalDetailsByCustomerId(int customerId);
        IDataResult<List<RentalDetailDto>> GetRentalDetailsByCarId(int carId);
        IResult Add(Rental rental);
        IResult Delete(Rental rental);
        IResult Update(Rental rental);
    }
}
