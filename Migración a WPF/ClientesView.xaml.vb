Imports MySql.Data.MySqlClient
Imports System.Configuration

Public Class ClientesView
    Inherits UserControl

    ' Cadena de conexión desde app.config
    Private connectionString As String = ConfigurationManager.ConnectionStrings("LoginConnection").ConnectionString

    ' Variable para almacenar el cliente seleccionado
    Private clienteSeleccionado As Cliente

    ' Constructor
    Public Sub New()
        InitializeComponent()
        CargarClientes() ' Cargar clientes al iniciar
    End Sub

    ' Método para cargar clientes desde la base de datos
    Private Sub CargarClientes()
        Dim query As String = "SELECT id, nombre, apellido, telefono, email FROM clientes"
        Dim clientes As New List(Of Cliente)

        Using connection As New MySqlConnection(connectionString)
            Using command As New MySqlCommand(query, connection)
                Try
                    connection.Open()
                    Dim reader As MySqlDataReader = command.ExecuteReader()

                    While reader.Read()
                        Dim cliente As New Cliente With {
                            .Id = If(reader.IsDBNull(reader.GetOrdinal("id")), 0, reader.GetInt32("id")),
                            .Nombre = If(reader.IsDBNull(reader.GetOrdinal("nombre")), String.Empty, reader.GetString("nombre")),
                            .Apellido = If(reader.IsDBNull(reader.GetOrdinal("apellido")), String.Empty, reader.GetString("apellido")),
                            .Telefono = If(reader.IsDBNull(reader.GetOrdinal("telefono")), String.Empty, reader.GetString("telefono")),
                            .Email = If(reader.IsDBNull(reader.GetOrdinal("email")), String.Empty, reader.GetString("email"))
                        }
                        clientes.Add(cliente)
                    End While
                Catch ex As Exception
                    MessageBox.Show("Error al cargar clientes: " & ex.Message)
                End Try
            End Using
        End Using

        dgClientes.ItemsSource = clientes ' Asignar la lista al DataGrid
    End Sub

    ' Evento para agregar un cliente
    Private Sub btnAgregar_Click(sender As Object, e As RoutedEventArgs)
        ' Mostrar el formulario y limpiar los campos
        FormularioCliente.Visibility = Visibility.Visible
        txtNombre.Text = ""
        txtApellido.Text = ""
        txtTelefono.Text = ""
        txtEmail.Text = ""
        clienteSeleccionado = Nothing ' No hay cliente seleccionado al agregar
    End Sub

    ' Evento para modificar un cliente
    Private Sub btnModificar_Click(sender As Object, e As RoutedEventArgs)
        clienteSeleccionado = TryCast(dgClientes.SelectedItem, Cliente)
        If clienteSeleccionado IsNot Nothing Then
            ' Mostrar el formulario y cargar los datos del cliente seleccionado
            FormularioCliente.Visibility = Visibility.Visible
            txtNombre.Text = clienteSeleccionado.Nombre
            txtApellido.Text = clienteSeleccionado.Apellido
            txtTelefono.Text = clienteSeleccionado.Telefono
            txtEmail.Text = clienteSeleccionado.Email
        Else
            MessageBox.Show("Seleccione un cliente para modificar.")
        End If
    End Sub

    ' Evento para eliminar un cliente
    Private Sub btnEliminar_Click(sender As Object, e As RoutedEventArgs)
        Dim clienteSeleccionado As Cliente = TryCast(dgClientes.SelectedItem, Cliente)
        If clienteSeleccionado IsNot Nothing Then
            Dim resultado As MessageBoxResult = MessageBox.Show("¿Está seguro de eliminar este cliente?", "Confirmar", MessageBoxButton.YesNo)
            If resultado = MessageBoxResult.Yes Then
                EliminarCliente(clienteSeleccionado.Id)
                CargarClientes() ' Recargar clientes después de eliminar
            End If
        Else
            MessageBox.Show("Seleccione un cliente para eliminar.")
        End If
    End Sub

    ' Método para eliminar un cliente
    Private Sub EliminarCliente(id As Integer)
        Dim query As String = "DELETE FROM clientes WHERE id = @id"

        Using connection As New MySqlConnection(connectionString)
            Using command As New MySqlCommand(query, connection)
                command.Parameters.AddWithValue("@id", id)
                Try
                    connection.Open()
                    command.ExecuteNonQuery()
                Catch ex As Exception
                    MessageBox.Show("Error al eliminar cliente: " & ex.Message)
                End Try
            End Using
        End Using
    End Sub

    ' Evento para guardar un cliente (agregar o modificar)
    Private Sub btnGuardar_Click(sender As Object, e As RoutedEventArgs)
        ' Validar que los campos no estén vacíos
        If String.IsNullOrWhiteSpace(txtNombre.Text) OrElse
           String.IsNullOrWhiteSpace(txtApellido.Text) OrElse
           String.IsNullOrWhiteSpace(txtTelefono.Text) OrElse
           String.IsNullOrWhiteSpace(txtEmail.Text) Then
            MessageBox.Show("Todos los campos son obligatorios.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            Return
        End If

        If clienteSeleccionado Is Nothing Then
            ' Agregar un nuevo cliente
            Dim nuevoCliente As New Cliente With {
                .Nombre = txtNombre.Text,
                .Apellido = txtApellido.Text,
                .Telefono = txtTelefono.Text,
                .Email = txtEmail.Text
            }
            InsertarCliente(nuevoCliente)
        Else
            ' Modificar el cliente seleccionado
            clienteSeleccionado.Nombre = txtNombre.Text
            clienteSeleccionado.Apellido = txtApellido.Text
            clienteSeleccionado.Telefono = txtTelefono.Text
            clienteSeleccionado.Email = txtEmail.Text
            ActualizarCliente(clienteSeleccionado)
        End If

        ' Ocultar el formulario y recargar la lista de clientes
        FormularioCliente.Visibility = Visibility.Collapsed
        CargarClientes() ' Recargar clientes después de guardar
    End Sub

    ' Método para insertar un nuevo cliente
    Private Sub InsertarCliente(cliente As Cliente)
        Dim query As String = "INSERT INTO clientes (nombre, apellido, telefono, email) VALUES (@nombre, @apellido, @telefono, @email)"

        Using connection As New MySqlConnection(connectionString)
            Using command As New MySqlCommand(query, connection)
                command.Parameters.AddWithValue("@nombre", cliente.Nombre)
                command.Parameters.AddWithValue("@apellido", cliente.Apellido)
                command.Parameters.AddWithValue("@telefono", cliente.Telefono)
                command.Parameters.AddWithValue("@email", cliente.Email)
                Try
                    connection.Open()
                    command.ExecuteNonQuery()
                    MessageBox.Show("Cliente agregado correctamente.") ' Mensaje de depuración
                Catch ex As Exception
                    MessageBox.Show("Error al agregar cliente: " & ex.Message)
                End Try
            End Using
        End Using
    End Sub

    ' Método para actualizar un cliente
    Private Sub ActualizarCliente(cliente As Cliente)
        Dim query As String = "UPDATE clientes SET nombre = @nombre, apellido = @apellido, telefono = @telefono, email = @email WHERE id = @id"

        Using connection As New MySqlConnection(connectionString)
            Using command As New MySqlCommand(query, connection)
                command.Parameters.AddWithValue("@nombre", cliente.Nombre)
                command.Parameters.AddWithValue("@apellido", cliente.Apellido)
                command.Parameters.AddWithValue("@telefono", cliente.Telefono)
                command.Parameters.AddWithValue("@email", cliente.Email)
                command.Parameters.AddWithValue("@id", cliente.Id)
                Try
                    connection.Open()
                    command.ExecuteNonQuery()
                    MessageBox.Show("Cliente modificado correctamente.") ' Mensaje de depuración
                Catch ex As Exception
                    MessageBox.Show("Error al modificar cliente: " & ex.Message)
                End Try
            End Using
        End Using
    End Sub

    ' Evento para cancelar la edición
    Private Sub btnCancelar_Click(sender As Object, e As RoutedEventArgs)
        ' Ocultar el formulario sin guardar cambios
        FormularioCliente.Visibility = Visibility.Collapsed
    End Sub

    ' Evento para manejar la selección en el DataGrid
    Private Sub dgClientes_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        btnModificar.IsEnabled = dgClientes.SelectedItem IsNot Nothing
        btnEliminar.IsEnabled = dgClientes.SelectedItem IsNot Nothing
    End Sub
End Class

' Clase para representar un cliente
Public Class Cliente
    Public Property Id As Integer
    Public Property Nombre As String
    Public Property Apellido As String
    Public Property Telefono As String
    Public Property Email As String
End Class