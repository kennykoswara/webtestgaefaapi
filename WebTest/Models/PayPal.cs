using PayPal.PayPalAPIInterfaceService;
using PayPal.PayPalAPIInterfaceService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using WebTest.Infrastructure;

namespace WebTest.Models
{
    public class PayPalExpressCheckout
    {
        public string TOKEN;
        public string PAYER_ID;
        public string TRANSACTION_ID;
        public string TOTAL_PRICE;

        public string CURRENCY_CODE;
        public string ITEM_CATEGORY;
        public string PAYMENT_ACTION_CODE;
        public string SOLUTION_TYPE;

        public string RETURN_URL="";
        public string CANCEL_URL="";

        public static string PayPalExpressCheckoutURL = "https://www.sandbox.paypal.com/cgi-bin/webscr?cmd=_express-checkout&token=";

        private static string NVP_API_USERNAME = GlobalVar.PAYPAL_NVP_API_USERNAME;
        private static string NVP_API_PASSWORD = GlobalVar.PAYPAL_NVP_API_PASSWORD;
        private static string NVP_API_SIGNATURE = GlobalVar.PAYPAL_NVP_API_SIGNATURE;
        private static string API_VERSION = "124.0";
        private static Dictionary<string, string> SDK_CONFIG;
        private static PayPalAPIInterfaceServiceService SERVICE;

        public PayPalExpressCheckout(string currCode = "USD", string itemCat = "Physical", string paymentAction = "Sale", string solType = "Sole")
        {
            CURRENCY_CODE = currCode;
            ITEM_CATEGORY = itemCat;
            PAYMENT_ACTION_CODE = paymentAction;
            SOLUTION_TYPE = solType;

            SDK_CONFIG = new Dictionary<string, string>();
            
            SDK_CONFIG.Add("mode", "sandbox");
            SDK_CONFIG.Add("account1.apiUsername", NVP_API_USERNAME);
            SDK_CONFIG.Add("account1.apiPassword", NVP_API_PASSWORD);
            SDK_CONFIG.Add("account1.apiSignature", NVP_API_SIGNATURE);

            SERVICE = new PayPalAPIInterfaceServiceService(SDK_CONFIG);
        }

        public bool SetExpressCheckout(string destination, double priceAdult, double priceChild, double priceChildNoBed, int adultQuantity, int childQuantity, int childNoBedQuantity, string couponCode, decimal discPrice, int discFlag, int discPercentage, int discType)
        {
            PaymentDetailsType paymentDetail = new PaymentDetailsType();
            CurrencyCodeType currency = (CurrencyCodeType)EnumUtils.GetValue(CURRENCY_CODE, typeof(CurrencyCodeType));
            PaymentDetailsItemType paymentItemAdult = new PaymentDetailsItemType();
            PaymentDetailsItemType paymentItemChild = new PaymentDetailsItemType();
            PaymentDetailsItemType paymentItemChildNoBed = new PaymentDetailsItemType();

            ///ADULT
            if (adultQuantity > 0)
            {
                paymentItemAdult.Name = "Tour Package to " + destination + " - Adult";
                double itemAmount = priceAdult;
                paymentItemAdult.Amount = new BasicAmountType(currency, itemAmount.ToString());
                int itemQuantity = adultQuantity;
                paymentItemAdult.Quantity = itemQuantity;
                //double taxCharges = Math.Round((((itemAmount + 0.3) * 1000 / 961) - itemAmount), 2);
                paymentItemAdult.ItemCategory = (ItemCategoryType)EnumUtils.GetValue(ITEM_CATEGORY, typeof(ItemCategoryType));
            }

            ///CHILD
            if (childQuantity > 0)
            {
                paymentItemChild.Name = "Tour Package to " + destination + " - Child";
                double itemAmount = priceChild;
                paymentItemChild.Amount = new BasicAmountType(currency, itemAmount.ToString());
                int itemQuantity = childQuantity;
                paymentItemChild.Quantity = itemQuantity;
                //double taxCharges = Math.Round((((itemAmount + 0.3) * 1000 / 961) - itemAmount), 2);
                paymentItemChild.ItemCategory = (ItemCategoryType)EnumUtils.GetValue(ITEM_CATEGORY, typeof(ItemCategoryType));
            }

            ///CHILDNOBED
            if (childNoBedQuantity > 0)
            {
                paymentItemChildNoBed.Name = "Tour Package to " + destination + " - Child Without Bed";
                double itemAmount = priceChildNoBed;
                paymentItemChildNoBed.Amount = new BasicAmountType(currency, itemAmount.ToString());
                int itemQuantity = childNoBedQuantity;
                paymentItemChildNoBed.Quantity = itemQuantity;
                //double taxCharges = Math.Round((((itemAmount + 0.3) * 1000 / 961) - itemAmount), 2);
                paymentItemChildNoBed.ItemCategory = (ItemCategoryType)EnumUtils.GetValue(ITEM_CATEGORY, typeof(ItemCategoryType));
            }

            double totalPrice = (adultQuantity * priceAdult) + (childQuantity * priceChild) + (childNoBedQuantity * priceChildNoBed);

            ////COUPON DISCOUNT/////
            PaymentDetailsItemType discount = new PaymentDetailsItemType();
            double discountAmount = 0.00;
            if (couponCode != "" || couponCode != null || discFlag == -1)
            {
                if(discType == 0)
                {
                    if (discFlag == 0)
                    {
                        discount.Name = "Coupon Discount - " + couponCode + " (" + discPercentage + "%)";
                    }
                    else if (discFlag == 1)
                    {
                        discount.Name = "Coupon Discount - " + couponCode + " (US$ " + discPrice + ")";
                    }
                }
                else
                {
                    if (discFlag == 0)
                    {
                        discount.Name = "Promo Discount - " + couponCode + " (" + discPercentage + "%)";
                    }
                    else if (discFlag == 1)
                    {
                        discount.Name = "Promo Discount - " + couponCode + " (US$ " + discPrice + ")";
                    }
                }
                
                discountAmount = Math.Round(0.00 - decimal.ToDouble(discPrice), 2, MidpointRounding.AwayFromZero);
                discount.Amount = new BasicAmountType(currency, discountAmount.ToString());
                int discQuantity = 1;
                discount.Quantity = discQuantity;
                discount.ItemCategory = (ItemCategoryType)EnumUtils.GetValue(ITEM_CATEGORY, typeof(ItemCategoryType));
            }
            ////////////////

            ///TAX///
            PaymentDetailsItemType tax = new PaymentDetailsItemType();
            tax.Name = "Paypal Tax";
            double taxAmount = Math.Round(Math.Round((((totalPrice + discountAmount) + 0.3) * 1000 / 961), 2, MidpointRounding.AwayFromZero) - Math.Round((totalPrice + discountAmount), 2, MidpointRounding.AwayFromZero), 2, MidpointRounding.AwayFromZero);
            tax.Amount = new BasicAmountType(currency, taxAmount.ToString());
            int taxQuantity = 1;
            tax.Quantity = taxQuantity;
            tax.ItemCategory = (ItemCategoryType)EnumUtils.GetValue(ITEM_CATEGORY, typeof(ItemCategoryType));
            ///
            

            List<PaymentDetailsItemType> paymentItems = new List<PaymentDetailsItemType>();
            if(adultQuantity > 0) paymentItems.Add(paymentItemAdult);
            if (childQuantity > 0) paymentItems.Add(paymentItemChild);
            if (childNoBedQuantity > 0) paymentItems.Add(paymentItemChildNoBed);

            if (couponCode != null)
            {
                paymentItems.Add(discount);
            }
            paymentItems.Add(tax);
            paymentDetail.PaymentDetailsItem = paymentItems;
            paymentDetail.PaymentAction = (PaymentActionCodeType)EnumUtils.GetValue(PAYMENT_ACTION_CODE, typeof(PaymentActionCodeType));
            paymentDetail.OrderTotal = new BasicAmountType((CurrencyCodeType)EnumUtils.GetValue(CURRENCY_CODE, typeof(CurrencyCodeType)), (totalPrice + discountAmount + taxAmount).ToString());
            

            List<PaymentDetailsType> paymentDetails = new List<PaymentDetailsType>();
            paymentDetails.Add(paymentDetail);

            SetExpressCheckoutRequestDetailsType ecDetails = new SetExpressCheckoutRequestDetailsType();
            ecDetails.SolutionType = (SolutionTypeType)EnumUtils.GetValue(SOLUTION_TYPE, typeof(SolutionTypeType));
            ecDetails.ReturnURL = RETURN_URL;
            ecDetails.CancelURL = CANCEL_URL;
            ecDetails.PaymentDetails = paymentDetails;
            ecDetails.AllowNote = "1"; //1 means allowed

            SetExpressCheckoutRequestType request = new SetExpressCheckoutRequestType();
            request.Version = API_VERSION;
            request.SetExpressCheckoutRequestDetails = ecDetails;

            SetExpressCheckoutReq wrapper = new SetExpressCheckoutReq();
            wrapper.SetExpressCheckoutRequest = request;

            SetExpressCheckoutResponseType setECResponse = SERVICE.SetExpressCheckout(wrapper);

            if (setECResponse.Ack.ToString() == "SUCCESS")
            {
                TOKEN = setECResponse.Token;
                return true;
            }
            else
            {
                return false;
            }
        }

        public GetExpressCheckoutDetailsResponseType GetExpressCheckout(string token)
        {
            GetExpressCheckoutDetailsRequestType request = new GetExpressCheckoutDetailsRequestType();
            request.Version = API_VERSION;
            request.Token = token;
            TOKEN = request.Token;
            GetExpressCheckoutDetailsReq wrapper = new GetExpressCheckoutDetailsReq();
            wrapper.GetExpressCheckoutDetailsRequest = request;

            GetExpressCheckoutDetailsResponseType ecResponse = SERVICE.GetExpressCheckoutDetails(wrapper);
            PAYER_ID = ecResponse.GetExpressCheckoutDetailsResponseDetails.PayerInfo.PayerID;
            TOTAL_PRICE = ecResponse.GetExpressCheckoutDetailsResponseDetails.PaymentDetails[0].OrderTotal.value;

            return ecResponse;
        }

        public DoExpressCheckoutPaymentResponseType DoExpressCheckout()
        {
            PaymentDetailsType paymentDetail = new PaymentDetailsType();
            paymentDetail.PaymentAction = (PaymentActionCodeType)EnumUtils.GetValue(PAYMENT_ACTION_CODE, typeof(PaymentActionCodeType));
            paymentDetail.OrderTotal = new BasicAmountType((CurrencyCodeType)EnumUtils.GetValue(CURRENCY_CODE, typeof(CurrencyCodeType)), TOTAL_PRICE);
            List<PaymentDetailsType> paymentDetails = new List<PaymentDetailsType>();
            paymentDetails.Add(paymentDetail);

            DoExpressCheckoutPaymentRequestType request = new DoExpressCheckoutPaymentRequestType();
            request.Version = API_VERSION;
            DoExpressCheckoutPaymentRequestDetailsType requestDetails = new DoExpressCheckoutPaymentRequestDetailsType();
            requestDetails.PaymentDetails = paymentDetails;
            requestDetails.Token = TOKEN;
            requestDetails.PayerID = PAYER_ID;
            request.DoExpressCheckoutPaymentRequestDetails = requestDetails;

            DoExpressCheckoutPaymentReq wrapper = new DoExpressCheckoutPaymentReq();
            wrapper.DoExpressCheckoutPaymentRequest = request;

            DoExpressCheckoutPaymentResponseType doECResponse = SERVICE.DoExpressCheckoutPayment(wrapper);
            return doECResponse;
            
        }

    }
}