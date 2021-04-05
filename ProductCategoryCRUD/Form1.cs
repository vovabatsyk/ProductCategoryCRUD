using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using ProductCategoryCRUD.Model;

namespace ProductCategoryCRUD
{
    public partial class Form1 : Form
    {
        private SqlConnection _connection;
        private SqlDataAdapter _adapter;
        private DataSet _dataSet;
        private int _productId = 0;

        public Form1()
        {
            InitializeComponent();
            string conStr = ConfigurationManager.ConnectionStrings["conStr"].ConnectionString;
            _connection = new SqlConnection(conStr);

            ConfigDataGridView();

            ShowData();

            InitCategory();
        }


        #region Events
        private void dataGridView1_Click(object sender, EventArgs e)
        {
            buttonDelete.Enabled = true;
            textBoxTitle.Text = dataGridView1.CurrentRow.Cells[2].Value.ToString();
            textBoxDesc.Text = dataGridView1.CurrentRow.Cells[3].Value.ToString();
            textBoxPrice.Text = dataGridView1.CurrentRow.Cells[4].Value.ToString();
            comboBoxCategory.Text = dataGridView1.CurrentRow.Cells[1].Value.ToString();

            buttonAdd.Text = "Update";

            _productId = Convert.ToInt32(dataGridView1.CurrentRow.Cells[0].Value);
        }
        private void buttonReset_Click(object sender, EventArgs e)
        {
            Reset();
            ShowData();
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            SearchData();
        }
        private void buttonFilter_Click(object sender, EventArgs e)
        {
            FilterData();
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (CheckFields())
            {
                try
                {
                    if (_connection.State == ConnectionState.Closed) _connection.Open();
                    if (buttonAdd.Text == "Add")
                    {

                        var category = comboBoxCategory.SelectedItem as Category;
                        SqlCommand command = new SqlCommand("AddProduct", _connection);
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@categoryId", category.Id);
                        command.Parameters.AddWithValue("@title", textBoxTitle.Text.Trim());
                        command.Parameters.AddWithValue("@description", textBoxDesc.Text.Trim());
                        command.Parameters.AddWithValue("@price", Convert.ToDouble(textBoxPrice.Text.Trim()));

                        command.ExecuteNonQuery();

                        ShowData();
                        Reset();

                    }
                    else if (buttonAdd.Text == "Update")
                    {
                        var category = comboBoxCategory.SelectedItem as Category;
                        SqlCommand command = new SqlCommand("UpdateProduct", _connection);
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@id", _productId);
                        command.Parameters.AddWithValue("@categoryId", category.Id);
                        command.Parameters.AddWithValue("@title", textBoxTitle.Text.Trim());
                        command.Parameters.AddWithValue("@description", textBoxDesc.Text.Trim());
                        command.Parameters.AddWithValue("@price", textBoxPrice.Text.Trim());

                        command.ExecuteNonQuery();

                        ShowData();
                        Reset();
                    }

                }
                catch (SqlException exception)
                {
                    MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    _connection.Close();
                }
            }
            else
            {
                MessageBox.Show("All fields is required!", "Opps!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (_connection.State == ConnectionState.Closed) _connection.Open();
                SqlCommand command = new SqlCommand("DeleteProduct", _connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@id", _productId);

                command.ExecuteNonQuery();

                Reset();
                ShowData();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _connection.Close();
            }
        }

        private void textBoxPrice_Leave(object sender, EventArgs e)
        {
            float parsedValue;
            if (!float.TryParse(textBoxPrice.Text, out parsedValue))
            {
                MessageBox.Show("This is a numeric field", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                textBoxPrice.Text = String.Empty;
            }

        }


        private void buttonNewCategory_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(textBoxNewCategory.Text))
            {
                try
                {
                    if (_connection.State == ConnectionState.Closed) _connection.Open();

                    SqlCommand command = new SqlCommand("AddCategory", _connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@name", textBoxNewCategory.Text.Trim());
                    command.ExecuteNonQuery();

                    InitCategory();
                    textBoxNewCategory.Text = String.Empty;
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
                finally { _connection.Close(); }
            }
            else
            {
                MessageBox.Show("This field is required!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(textBoxUpdateCategory.Text))
            {

                try
                {
                    if (_connection.State == ConnectionState.Closed) _connection.Open();

                    var category = comboBoxEditCategory.SelectedItem as Category;
                    SqlCommand command = new SqlCommand("UpdateCategory", _connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@id", category.Id);

                    command.Parameters.AddWithValue("@name", textBoxUpdateCategory.Text.Trim());

                    command.ExecuteNonQuery();

                    InitCategory();
                    ShowData();
                    textBoxUpdateCategory.Text = String.Empty;

                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
                finally { _connection.Close(); }
            }
            else
            {
                MessageBox.Show("This field is required!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        #endregion

        #region Methods
        private void ShowData()
        {
            try
            {
                _adapter = new SqlDataAdapter("ShowAllProducts", _connection);
                _adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                _dataSet = new DataSet();
                _adapter.Fill(_dataSet, "Products");
                dataGridView1.DataSource = _dataSet.Tables["Products"];

                dataGridView1.Columns[0].Visible = false;

            }
            catch (SqlException e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _connection.Close();
            }
        }
        private void Reset()
        {
            buttonDelete.Enabled = false;
            textBoxPrice.Text = String.Empty;
            textBoxDesc.Text = String.Empty;
            textBoxTitle.Text = String.Empty;
            textBoxSearch.Text = String.Empty;
            buttonAdd.Text = "Add";
            comboBoxCategory.Text = comboBoxCategory.Items[0].ToString();
            comboBoxFilter.Text = comboBoxCategory.Items[0].ToString();
            _productId = 0;
        }
        private void SearchData()
        {
            try
            {
                if (_connection.State == ConnectionState.Closed) _connection.Open();

                _adapter = new SqlDataAdapter("SearchProduct", _connection);
                _adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                _adapter.SelectCommand.Parameters.AddWithValue("@title", textBoxSearch.Text.Trim());

                _dataSet = new DataSet();
                _adapter.Fill(_dataSet, "Products");
                dataGridView1.DataSource = _dataSet.Tables["Products"];
            }
            catch (SqlException e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _connection.Close();
            }
        }

        private void FilterData()
        {
            try
            {
                if (_connection.State == ConnectionState.Closed) _connection.Open();

                if (comboBoxFilter.Text == "All Products")
                {
                    ShowData();
                }
                else
                {
                    _adapter = new SqlDataAdapter("FilterByCategory", _connection);
                    _adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    _adapter.SelectCommand.Parameters.AddWithValue("@categoryName", comboBoxFilter.Text.Trim());

                    _dataSet = new DataSet();
                    _adapter.Fill(_dataSet, "Products");
                    dataGridView1.DataSource = _dataSet.Tables["Products"];

                    dataGridView1.Columns[0].Visible = false;

                }

            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _connection.Close();
            }
        }

        private void ConfigDataGridView()
        {
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        private bool CheckFields()
        {
            if (String.IsNullOrEmpty(textBoxTitle.Text) ||
                String.IsNullOrEmpty(textBoxDesc.Text) ||
                String.IsNullOrEmpty(textBoxPrice.Text))
            {
                return false;
            }

            return true;
        }

        private void InitCategory()
        {
            comboBoxCategory.Items.Clear();
            comboBoxEditCategory.Items.Clear();
            comboBoxFilter.Items.Clear();
            try
            {
                if (_connection.State == ConnectionState.Closed) _connection.Open();
                SqlCommand command = new SqlCommand();
                command.CommandText = "select id, categoryName from Category";
                command.Connection = _connection;
                SqlDataReader reader = command.ExecuteReader();
                comboBoxFilter.Items.Add("All Products");
                while (reader.Read())
                {
                    comboBoxCategory.Items.Add(new Category() { Id = (int)reader["id"], Name = reader["categoryName"].ToString() });
                    comboBoxFilter.Items.Add(new Category() { Id = (int)reader["id"], Name = reader["categoryName"].ToString() });
                    comboBoxEditCategory.Items.Add(new Category() { Id = (int)reader["id"], Name = reader["categoryName"].ToString() });

                }

                comboBoxCategory.Text = comboBoxCategory.Items[0].ToString();
                comboBoxEditCategory.Text = comboBoxCategory.Items[0].ToString();
                comboBoxFilter.Text = "All Products";

            }
            catch (SqlException e)
            {
                MessageBox.Show(e.ToString());
            }
            finally
            {
                _connection.Close();
            }
        }


        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }
    }
}
