using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace wt4u.BusinessLogic
{
    public class wt4uTransaction
    {
        public delegate ActionResult transactionDelegate();
        public ActionResult transact(transactionDelegate del)
        {
            ActionResult result = new RedirectToRouteResult(new RouteValueDictionary(new { action = "Index" }));;
            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    result = del();
                    scope.Complete();
                }
            }
            catch (TransactionAbortedException ex)
            {
                return new RedirectToRouteResult(new RouteValueDictionary(new { action = "Index", Error = "Transaction aborted " + ex.Message }));
            }
            catch (ApplicationException ex)
            {
                return new RedirectToRouteResult(new RouteValueDictionary(new { action = "Index", Error = ex.Message }));
            }
            return result;
        }
    }
}