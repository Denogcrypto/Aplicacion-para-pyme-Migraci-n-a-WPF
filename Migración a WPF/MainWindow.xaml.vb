Imports System.Configuration
Imports MySql.Data.MySqlClient
Imports System.Windows ' Para MessageBox en WPF

Class MainWindow

    ' Método para obtener la cadena de conexión desde app.config
    Private Function ObtenerCadenaConexion() As String
        Return ConfigurationManager.ConnectionStrings("LoginConnection").ConnectionString
    End Function

    ' Método para validar las credenciales en la base de datos
    Private Function ValidarCredenciales(username As String, password As String) As Boolean
        Try
            Dim connectionString As String = ObtenerCadenaConexion()
            Using conn As New MySqlConnection(connectionString)
                conn.Open()
                Dim query As String = "SELECT COUNT(*) FROM usuarios WHERE username = @username AND password = @password"
                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@username", username)
                    cmd.Parameters.AddWithValue("@password", password)
                    Dim count As Integer = Convert.ToInt32(cmd.ExecuteScalar())
                    Return count > 0
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error al validar credenciales: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            Return False
        End Try
    End Function

    ' Evento para mover la ventana al hacer clic y arrastrar
    Private Sub Window_MouseDown(sender As Object, e As MouseButtonEventArgs) Handles Me.MouseDown
        If e.LeftButton = MouseButtonState.Pressed Then
            DragMove()
        End If
    End Sub

    ' Evento para minimizar la ventana
    Private Sub btnMinimize_Click(sender As Object, e As RoutedEventArgs)
        WindowState = WindowState.Minimized
    End Sub

    ' Evento para cerrar la aplicación
    Private Sub btnClosed_Click(sender As Object, e As RoutedEventArgs)
        Application.Current.Shutdown()
    End Sub

    ' Evento para el botón de inicio de sesión
    Private Sub btnLogin_Click(sender As Object, e As RoutedEventArgs)
        ' Obtener el usuario y la contraseña ingresados
        Dim username As String = txtUser.Text
        Dim password As String = txtPassword.Password

        ' Validar si los campos están vacíos
        If String.IsNullOrEmpty(username) OrElse String.IsNullOrEmpty(password) Then
            MessageBox.Show("Por favor, ingresa tu usuario y contraseña.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning)
            Return
        End If

        ' Validar las credenciales en la base de datos
        If ValidarCredenciales(username, password) Then
            MessageBox.Show("Inicio de sesión exitoso.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information)

            ' Abrir el formulario Menu.xaml
            Dim menuForm As New Menu()
            menuForm.Show() ' Muestra el formulario Menu.xaml

            ' Cerrar el formulario actual (MainWindow.xaml)
            Me.Close()
        Else
            MessageBox.Show("Usuario o contraseña incorrectos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
        End If
    End Sub

    ' Evento TextChanged para el TextBox de usuario (opcional)
    Private Sub txtUser_TextChanged(sender As Object, e As TextChangedEventArgs)

    End Sub
End Class