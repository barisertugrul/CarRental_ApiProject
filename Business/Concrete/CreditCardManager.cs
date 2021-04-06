using System;
using System.Collections.Generic;
using System.Text;
using Business.Abstract;
using Business.Constants;
using Business.ValidationRules.FluentValidation;
using Core.Aspects.Autofac.Caching;
using Core.Aspects.Autofac.Validation;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;

namespace Business.Concrete
{
    public class CreditCardManager:ICreditCardService
    {
        private ICreditCardDal _creditCardDal;


        public CreditCardManager(ICreditCardDal creditCardDal)
        {
            _creditCardDal = creditCardDal;
        }


        public IDataResult<List<CreditCard>> GetAll()
        {
            throw new NotImplementedException();
        }

        public IDataResult<CreditCard> GetById(int id)
        {
            throw new NotImplementedException();
        }

        public IDataResult<List<CreditCard>> GetCardsByCustomerId(int customerId)
        {
            throw new NotImplementedException();
        }

        public IDataResult<List<CreditCard>> GetCardsByName(string cardName)
        {
            throw new NotImplementedException();
        }


        [ValidationAspect(typeof(CreditCardValidator))]
        [CacheRemoveAspect("ICreditCardService.Get")]
        public IResult Add(CreditCard creditCard)
        {
            //TODO Kayıtlı kart kontrolü
            _creditCardDal.Add(creditCard);
            return new SuccessResult(Messages.NewCardAdded);
        }

        public IResult Delete(CreditCard creditCard)
        {
            throw new NotImplementedException();
        }

        public IResult Update(CreditCard creditCard)
        {
            throw new NotImplementedException();
        }
    }
}
