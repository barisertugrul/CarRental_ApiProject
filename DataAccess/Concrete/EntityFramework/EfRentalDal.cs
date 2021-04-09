﻿using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DataAccess.Concrete.EntityFramework
{
    public class EfRentalDal : EfEntityRepositoryBase<Rental, RentACarContext>, IRentalDal
    {
        public List<RentalDetailDto> GetAllDetails()
        {
            using (RentACarContext context = new RentACarContext())
            {
                var result = from r in context.Rentals
                             join c in context.Customers
                             on r.CustomerId equals c.Id
                             join cr in context.Cars
                             on r.CarId equals cr.CarId
                             join b in context.Brands
                             on cr.BrandId equals b.Id
                             join cl in context.Colors
                             on cr.ColorId equals cl.Id
                             join u in context.Users
                             on c.UserId equals u.Id
                             select new RentalDetailDto
                             {
                                 Id = r.Id,
                                 CarName = cr.CarName,
                                 BrandName = b.Name,
                                 ColorName = cl.Name,
                                 FirstName = u.FirstName,
                                 LastName = u.LastName,
                                 Email = u.Email,
                                 CompanyName = c.CompanyName,
                                 RentStartDate = r.RentStartDate,
                                 RentEndDate = r.RentEndDate,
                                 ReturnDate = r.ReturnDate,
                                 Confirm = r.Confirm
                             };
                return result.ToList();
            }
        }

        public RentalDetailDto GetDetails(Expression<Func<Rental, bool>> filter)
        {
            return GetAllDetailsBy(filter).SingleOrDefault();
        }

        public List<RentalDetailDto> GetAllDetailsBy(Expression<Func<Rental, bool>> filter)
        {
            using (RentACarContext context = new RentACarContext())
            {
                var result = from r in context.Rentals.Where(filter)
                             join c in context.Customers
                             on r.CustomerId equals c.Id
                             join u in context.Users
                             on c.Id equals u.Id
                             join cr in context.Cars
                             on r.CarId equals cr.CarId
                             join b in context.Brands
                             on cr.BrandId equals b.Id
                             join cl in context.Colors
                             on cr.ColorId equals cl.Id
                             select new RentalDetailDto
                             {
                                 Id = r.Id,
                                 CarName = cr.CarName,
                                 BrandName = b.Name,
                                 ColorName = cl.Name,
                                 FirstName = u.FirstName,
                                 LastName = u.LastName,
                                 Email = u.Email,
                                 CompanyName = c.CompanyName,
                                 RentStartDate = r.RentStartDate,
                                 RentEndDate = r.RentEndDate,
                                 ReturnDate = r.ReturnDate,
                                 Confirm = r.Confirm
                             };
                return result.ToList();
            }
        }
    }
}
