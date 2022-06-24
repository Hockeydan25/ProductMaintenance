using System;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;    //for SQlException object
using ProductMaintenance.Models.DataLayer;

namespace ProductMaintenance
{
    public partial class frmProductMaintenance : Form
    {

        /// <summary>
        /// Product object that will store the data from the Products table. 
        /// </summary>
        public frmProductMaintenance()
        {
            InitializeComponent();
        }

        Product selectedProduct;

        /// <summary>
        /// Method GedtProduct,with in try and catch call for data validation and 
        /// string product code for textbox declarion. Product is null true/false
        /// displayProduct is called.
        /// </summary>
        /// <displayProduct>Method parameters for display from the OrderOpion properties textboxes.
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void btnGetProduct_Click(object sender, EventArgs e)
        {
            if (IsValidData())
            {
                try
                {
                    string productCode = txtProductCode.Text;
                    selectedProduct = ProductDB.GetProduct(productCode);
                
                    if (selectedProduct == null)
                    {
                        MessageBox.Show("No Product Code found with the Product code entered. " +
                            "Please try your luck again, or look up codes in our Catalog.", "Product Not Found");
                        this.ClearControls();
                    }
                    else
                    {
                        this.DisplayProduct();
                    }
                }
                catch (SqlException ex)
                {
                    this.HandleDatabaseError(ex);
                }
                catch (Exception ex)
                {
                    this.HandleGeneralError(ex);
                } 
            }
        }

        /// <summary>
        /// Isvaild data method for each property is called to check its present.
        /// </summary>
        /// <returns></returns>
        private bool IsValidData()
        {
            bool success = true;
            string errorMessage = "";
            errorMessage = Validator.IsPresent(txtProductCode.Text, txtProductCode.Tag.ToString());
            if (errorMessage != "")
            {
                success = false;
                MessageBox.Show(errorMessage, "Entry Error");
                txtProductCode.Focus();
            }
            return success;
        }

        /// <summary>
        /// Method assigns values, parameters are loaded into textbox for 
        /// appropriate properties of the OrderOptions form. Converts to appropriate
        /// object to decimal in this case all integers for this program. 
        /// </summary>
        private void DisplayProduct()
        {
            txtProductCode.Text = selectedProduct.ProductCode;
            txtDescription.Text = selectedProduct.Description;
            txtUnitPrice.Text = selectedProduct.UnitPrice.ToString("c");
            txtOnHand.Text = selectedProduct.OnHandQuantity.ToString();

            txtProductCode.Focus();
        }

        /// <summary>
        /// Method empties text boxes on the form disables controls set focus back to productCode.
        /// </summary>
        private void ClearControls()
        {
            txtProductCode.Text = "";
            txtDescription.Text = "";
            txtUnitPrice.Text = "";
            txtOnHand.Text = "";
            txtProductCode.Focus();
        }

        /// <summary>
        /// dialogbox to add product with parameters diplayed. Next accepted changes
        /// could bve a success message in here. addmodifyProduct selected product , try and catch.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdd_Click(object sender, EventArgs e)
        {
            frmAddModify addModifyForm = new frmAddModify
            {
                AddProduct = true
            };
            DialogResult result = addModifyForm.ShowDialog();

            if (result == DialogResult.OK)
            {
                try
                {
                    selectedProduct = addModifyForm.Product;
                    ProductDB.AddProduct(selectedProduct);
                    this.DisplayProduct();
                }
                catch (SqlException ex)
                {
                    this.HandleDatabaseError(ex);
                }
                catch (Exception ex)
                {
                    this.HandleGeneralError(ex);
                }  
            }
        }

        /// <summary>
        /// dialogbox to modify product with parameters diplayed. Next accepted changes
        /// could bve a success message in here. addmodifyProduct selected product , try and catch.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnModify_Click(object sender, EventArgs e)
        {
            Product oldProduct = this.CloneProduct();  //save old values
            frmAddModify addModifyForm = new frmAddModify
            {
                AddProduct = false,
                Product = selectedProduct 
            };
            DialogResult result = addModifyForm.ShowDialog();

            if (result == DialogResult.OK)
            {
                try
                {                                           //get new values
                    selectedProduct = addModifyForm.Product;
                    if(ProductDB.UpdateProduct(oldProduct, selectedProduct))
                    {
                        this.DisplayProduct();
                    }
                    else
                    {
                        this.HandleConcurrencyConflict();
                    }
                }
                catch (SqlException ex)
                {
                    this.HandleDatabaseError(ex);
                }
                catch (Exception ex)
                {
                    this.HandleGeneralError(ex);
                }
            }
        }

        /// <summary>
        /// Method makes copy of original database data past to the addmodify form method.
        /// </summary>
        /// <returns></returns>
        private Product CloneProduct()
        {
            return new Product()
            {
                ProductCode = selectedProduct.ProductCode,
                Description = selectedProduct.Description,
                UnitPrice = selectedProduct.UnitPrice,
                OnHandQuantity = selectedProduct.OnHandQuantity
            };
        }

        /// <summary>
        /// dialogbox to delete product with parameters diplayed. Next accepted changes
        /// could bve a success message in here. saves selected product , try and catch.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelete_Click(object sender, EventArgs e)
        {
            var desc = selectedProduct.Description; 
            DialogResult result =
                MessageBox.Show($"Delete {desc}?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                try
                {
                    if (ProductDB.DeleteProduct(selectedProduct))
                    {
                        this.ClearControls();  
                    }
                    else
                    {
                        this.HandleConcurrencyConflict();
                    }
                }
                catch (SqlException ex)
                {
                    this.HandleDatabaseError(ex);
                }
                catch (Exception ex)
                {
                    this.HandleGeneralError(ex);
                }
            }
        }

        /// <summary>
        /// reload customer, check if option variable is null, if yes notify user it has been
        /// deleted by another users.
        /// </summary>
        private void HandleConcurrencyConflict()
        {
            selectedProduct = ProductDB.GetProduct(selectedProduct.ProductCode); // reload
            if (selectedProduct == null)
            {
                MessageBox.Show("Another user has deleted that product.", //message for any deleted product
                    "Concurrency Error");
                this.ClearControls();
            }
            else    // messagefor updates
            {
                string message = "Another user has updated that product.\n" +
                    "The current database values will be displayed.";
                MessageBox.Show(message, "Concurrency Error");
                this.DisplayProduct();
            }
        }

        /// <summary>
        /// SQLDatabase errors, can writ to etc..duplicate or Unique indentifier 
        /// </summary>
        /// <param name="ex"></param>
        private void HandleDatabaseError(SqlException ex)
        {
            MessageBox.Show(ex.Message, ex.GetType().ToString());
        }

        /// <summary>
        /// Handles other errors. 
        /// </summary>
        /// <param name="ex"></param>
        private void HandleGeneralError(Exception ex) 
        {
            MessageBox.Show(ex.Message, ex.GetType().ToString());
        }

        /// <summary>
        /// closes program, end.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExit_Click(object sender, EventArgs e)//exit program 
        {
            this.Close();
        }
    }
}
