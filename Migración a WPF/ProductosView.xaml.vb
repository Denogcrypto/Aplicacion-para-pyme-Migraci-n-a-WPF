Imports MySql.Data.MySqlClient
Imports System.Configuration

Public Class ProductosView
    Inherits UserControl

    ' Cadena de conexión desde app.config
    Private connectionString As String = ConfigurationManager.ConnectionStrings("LoginConnection").ConnectionString

    ' Variable para almacenar el producto seleccionado
    Private productoSeleccionado As Producto

    ' Constructor
    Public Sub New()
        InitializeComponent()
        CargarProductos() ' Cargar productos al iniciar
    End Sub

    ' Método para cargar productos desde la base de datos
    Private Sub CargarProductos()
        Dim query As String = "SELECT id, nombre, descripcion, precio, categoria, stock FROM productos"
        Dim productos As New List(Of Producto)

        Using connection As New MySqlConnection(connectionString)
            Using command As New MySqlCommand(query, connection)
                Try
                    connection.Open()
                    Dim reader As MySqlDataReader = command.ExecuteReader()

                    While reader.Read()
                        Dim producto As New Producto With {
                            .Id = If(reader.IsDBNull(reader.GetOrdinal("id")), 0, reader.GetInt32("id")),
                            .Nombre = If(reader.IsDBNull(reader.GetOrdinal("nombre")), String.Empty, reader.GetString("nombre")),
                            .Descripcion = If(reader.IsDBNull(reader.GetOrdinal("descripcion")), String.Empty, reader.GetString("descripcion")),
                            .Precio = If(reader.IsDBNull(reader.GetOrdinal("precio")), 0.0, reader.GetDecimal("precio")),
                            .Categoria = If(reader.IsDBNull(reader.GetOrdinal("categoria")), String.Empty, reader.GetString("categoria")),
                            .Stock = If(reader.IsDBNull(reader.GetOrdinal("stock")), 0, reader.GetInt32("stock"))
                        }
                        productos.Add(producto)
                    End While
                Catch ex As Exception
                    MessageBox.Show("Error al cargar productos: " & ex.Message)
                End Try
            End Using
        End Using

        dgProductos.ItemsSource = productos ' Asignar la lista al DataGrid
    End Sub

    ' Evento para agregar un producto
    Private Sub btnAgregar_Click(sender As Object, e As RoutedEventArgs)
        ' Mostrar el formulario y limpiar los campos
        FormularioProducto.Visibility = Visibility.Visible
        txtNombre.Text = ""
        txtDescripcion.Text = ""
        txtPrecio.Text = ""
        txtCategoria.Text = ""
        txtStock.Text = ""
        productoSeleccionado = Nothing ' No hay producto seleccionado al agregar
    End Sub

    ' Evento para modificar un producto
    Private Sub btnModificar_Click(sender As Object, e As RoutedEventArgs)
        productoSeleccionado = TryCast(dgProductos.SelectedItem, Producto)
        If productoSeleccionado IsNot Nothing Then
            ' Mostrar el formulario y cargar los datos del producto seleccionado
            FormularioProducto.Visibility = Visibility.Visible
            txtNombre.Text = productoSeleccionado.Nombre
            txtDescripcion.Text = productoSeleccionado.Descripcion
            txtPrecio.Text = productoSeleccionado.Precio.ToString()
            txtCategoria.Text = productoSeleccionado.Categoria
            txtStock.Text = productoSeleccionado.Stock.ToString()
        Else
            MessageBox.Show("Seleccione un producto para modificar.")
        End If
    End Sub

    ' Evento para eliminar un producto
    Private Sub btnEliminar_Click(sender As Object, e As RoutedEventArgs)
        Dim productoSeleccionado As Producto = TryCast(dgProductos.SelectedItem, Producto)
        If productoSeleccionado IsNot Nothing Then
            Dim resultado As MessageBoxResult = MessageBox.Show("¿Está seguro de eliminar este producto?", "Confirmar", MessageBoxButton.YesNo)
            If resultado = MessageBoxResult.Yes Then
                EliminarProducto(productoSeleccionado.Id)
                CargarProductos() ' Recargar productos después de eliminar
            End If
        Else
            MessageBox.Show("Seleccione un producto para eliminar.")
        End If
    End Sub

    ' Método para eliminar un producto
    Private Sub EliminarProducto(id As Integer)
        Dim query As String = "DELETE FROM productos WHERE id = @id"

        Using connection As New MySqlConnection(connectionString)
            Using command As New MySqlCommand(query, connection)
                command.Parameters.AddWithValue("@id", id)
                Try
                    connection.Open()
                    command.ExecuteNonQuery()
                Catch ex As Exception
                    MessageBox.Show("Error al eliminar producto: " & ex.Message)
                End Try
            End Using
        End Using
    End Sub

    ' Evento para guardar un producto (agregar o modificar)
    Private Sub btnGuardar_Click(sender As Object, e As RoutedEventArgs)
        ' Validar que los campos no estén vacíos
        If String.IsNullOrWhiteSpace(txtNombre.Text) OrElse
           String.IsNullOrWhiteSpace(txtDescripcion.Text) OrElse
           String.IsNullOrWhiteSpace(txtPrecio.Text) OrElse
           String.IsNullOrWhiteSpace(txtCategoria.Text) OrElse
           String.IsNullOrWhiteSpace(txtStock.Text) Then
            MessageBox.Show("Todos los campos son obligatorios.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            Return
        End If

        ' Validar que el precio y el stock sean números válidos
        Dim precio As Decimal
        Dim stock As Integer
        If Not Decimal.TryParse(txtPrecio.Text, precio) OrElse Not Integer.TryParse(txtStock.Text, stock) Then
            MessageBox.Show("El precio y el stock deben ser números válidos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            Return
        End If

        If productoSeleccionado Is Nothing Then
            ' Agregar un nuevo producto
            Dim nuevoProducto As New Producto With {
                .Nombre = txtNombre.Text,
                .Descripcion = txtDescripcion.Text,
                .Precio = precio,
                .Categoria = txtCategoria.Text,
                .Stock = stock
            }
            InsertarProducto(nuevoProducto)
        Else
            ' Modificar el producto seleccionado
            productoSeleccionado.Nombre = txtNombre.Text
            productoSeleccionado.Descripcion = txtDescripcion.Text
            productoSeleccionado.Precio = precio
            productoSeleccionado.Categoria = txtCategoria.Text
            productoSeleccionado.Stock = stock
            ActualizarProducto(productoSeleccionado)
        End If

        ' Ocultar el formulario y recargar la lista de productos
        FormularioProducto.Visibility = Visibility.Collapsed
        CargarProductos() ' Recargar productos después de guardar
    End Sub

    ' Método para insertar un nuevo producto
    Private Sub InsertarProducto(producto As Producto)
        Dim query As String = "INSERT INTO productos (nombre, descripcion, precio, categoria, stock) VALUES (@nombre, @descripcion, @precio, @categoria, @stock); SELECT LAST_INSERT_ID();"

        Using connection As New MySqlConnection(connectionString)
            Using command As New MySqlCommand(query, connection)
                command.Parameters.AddWithValue("@nombre", producto.Nombre)
                command.Parameters.AddWithValue("@descripcion", producto.Descripcion)
                command.Parameters.AddWithValue("@precio", producto.Precio)
                command.Parameters.AddWithValue("@categoria", producto.Categoria)
                command.Parameters.AddWithValue("@stock", producto.Stock)

                Try
                    connection.Open()
                    Dim nuevoId As Integer = Convert.ToInt32(command.ExecuteScalar())
                    MessageBox.Show("Producto agregado correctamente. ID: " & nuevoId)
                Catch ex As Exception
                    MessageBox.Show("Error al agregar producto: " & ex.Message)
                End Try
            End Using
        End Using
    End Sub

    ' Método para actualizar un producto
    Private Sub ActualizarProducto(producto As Producto)
        Dim query As String = "UPDATE productos SET nombre = @nombre, descripcion = @descripcion, precio = @precio, categoria = @categoria, stock = @stock WHERE id = @id"

        Using connection As New MySqlConnection(connectionString)
            Using command As New MySqlCommand(query, connection)
                command.Parameters.AddWithValue("@nombre", producto.Nombre)
                command.Parameters.AddWithValue("@descripcion", producto.Descripcion)
                command.Parameters.AddWithValue("@precio", producto.Precio)
                command.Parameters.AddWithValue("@categoria", producto.Categoria)
                command.Parameters.AddWithValue("@stock", producto.Stock)
                command.Parameters.AddWithValue("@id", producto.Id)

                Try
                    connection.Open()
                    command.ExecuteNonQuery()
                    MessageBox.Show("Producto modificado correctamente.")
                Catch ex As Exception
                    MessageBox.Show("Error al modificar producto: " & ex.Message)
                End Try
            End Using
        End Using
    End Sub

    ' Evento para cancelar la edición
    Private Sub btnCancelar_Click(sender As Object, e As RoutedEventArgs)
        ' Ocultar el formulario sin guardar cambios
        FormularioProducto.Visibility = Visibility.Collapsed
    End Sub

    ' Evento para manejar la selección en el DataGrid
    Private Sub dgProductos_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        btnModificar.IsEnabled = dgProductos.SelectedItem IsNot Nothing
        btnEliminar.IsEnabled = dgProductos.SelectedItem IsNot Nothing
    End Sub
End Class

' Clase para representar un producto
Public Class Producto
    Public Property Id As Integer
    Public Property Nombre As String
    Public Property Descripcion As String
    Public Property Precio As Decimal
    Public Property Categoria As String
    Public Property Stock As Integer
End Class