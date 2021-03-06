﻿using CMS.Models.Data;
using CMS.Models.ViewModels.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;

namespace CMS.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {
            // Initalizating cart
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            // Checking whether our cart is empty
            if (cart.Count == 0 || Session["cart"] == null)
            {
                ViewBag.Message = "Your cart is empty!";
                return View();
            }

            // Counting total and passing it to ViewBag
            decimal total = 0m;
            foreach (var item in cart)
            {
                total += item.Total;
            }

            ViewBag.GrandTotal = total;
            return View(cart);
        }

        public ActionResult CartPartial()
        {
            // Initalize CartVM
            CartVM model = new CartVM();

            // Initalize Quantity and Price
            int quantity = 0;
            decimal price = 0;
            
            // Check whether we have cart data saved in current session
            if (Session["cart"] != null)
            {
                // Fetching data from session
                var list = (List<CartVM>)Session["cart"];

                foreach (var item in list)
                {
                    quantity += item.Quantity;
                    price += item.Quantity * item.Price;
                }

                model.Quantity = quantity;
                model.Price = price;
            } else
            {
                // Resetting values to default when cart is empty
                quantity = 0;
                price = 0m;
            }

            return PartialView(model);
        }

        public ActionResult AddToCartPartial(int id)
        {
            // Initalize CartVM List
            List<CartVM> cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            // Initalize model
            CartVM model = new CartVM();
            using (DB db = new DB())
            {
                // Fetching product to add
                ProductDTO product = db.Products.Find(id);

                // Checking whether this item is not present in the cart already
                var productInCart = cart.FirstOrDefault(x => x.ProductId == id);

                // Determine whether to add product's quantity or to add a new product to cartt
                if (productInCart == null)
                {
                    cart.Add(new CartVM()
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 1,
                        Price = product.Price,
                        Image = product.ImageName
                    });
                } else
                {
                    productInCart.Quantity++;
                }
            }
            // Declaring return variables
            int quantity = 0;
            decimal price = 0;

            // Iterating over cart list and calculating Quantity and Price
            foreach (var item in cart)
            {
                quantity += item.Quantity;
                price += item.Quantity * item.Price;
            }
            model.Quantity = quantity;
            model.Price = price;

            // Save data in session
            Session["cart"] = cart;
            return PartialView(model);
        }

        public JsonResult IncrementProduct(int productId)
        {
            // Initialize list CartVM
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            // Fetching CartVM
            CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

            // Incrementing quantity of product
            model.Quantity++;

            // Getting data ready to return
            var result = new { quantity = model.Quantity, price = model.Price };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DecrementProduct(int productId)
        {
            // Initialize list CartVM
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            // Fetching CartVM
            CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

            // Incrementing quantity of product
            if (model.Quantity > 1)
            {
                model.Quantity--;
            } else
            {
                model.Quantity = 0;
                cart.Remove(model);
            }
           
            // Getting data ready to return
            var result = new { quantity = model.Quantity, price = model.Price };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public void RemoveProduct(int productId)
        {
            // Initialize list CartVM
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            // Fetching CartVM
            CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

            // Delete product
            cart.Remove(model);
        }

        public ActionResult PayPalPartial()
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            return PartialView(cart);
        }

        [HttpPost]
        public void PlaceOrder()
        {
            // Fetching contents of the cart in session
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            // Get user name
            string username = User.Identity.Name;

            // Declaring number of order
            int orderId = 0;

            using (DB db = new DB())
            {
                // Initialize OrderDTO
                OrderDTO orderDTO = new OrderDTO();

                // Get UserId
                var user = db.Users.FirstOrDefault(x => x.UserName == username);
                int userId = user.Id;

                // Set OrderDTO and save 
                orderDTO.UserId = userId;
                orderDTO.CreatedAt = DateTime.Now;

                db.Orders.Add(orderDTO);
                db.SaveChanges();

                // Fetching ID of saved order
                orderId = orderDTO.OrderId;

                // Initialize OrderDetailsDTO
                OrderDetailsDTO orderDetailsDTO = new OrderDetailsDTO();

                foreach (var item in cart)
                {
                    orderDetailsDTO.OrderId = orderId;
                    orderDetailsDTO.UserId = userId;
                    orderDetailsDTO.ProductId = item.ProductId;
                    orderDetailsDTO.Quantity = item.Quantity;

                    db.OrderDetails.Add(orderDetailsDTO);
                    db.SaveChanges();
                }
            }

            // Send email to ADMIN that new order is active
            var client = new SmtpClient("smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("405f212db24e40", "6bbf21865479f6"),
                EnableSsl = true
            };
            client.Send("admin@example.com", "admin@example.com", "New Order Placed", "New order has been created on your CMS \n OrderId: " + orderId);

            // Reset session
            Session["cart"] = null;
        }
    }
}