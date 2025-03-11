Imports System.Runtime.InteropServices
Imports System.Windows.Interop
Imports MySql.Data.MySqlClient
Imports System.Configuration
Imports System.ComponentModel ' Necesario para INotifyPropertyChanged

Public Class Menu
    Inherits Window
    Implements INotifyPropertyChanged ' Implementa la interfaz

    ' Propiedad para el nombre de usuario
    Private _username As String
    Public Property Username As String
        Get
            Return _username
        End Get
        Set(value As String)
            _username = value
            NotifyPropertyChanged(NameOf(Username)) ' Notifica el cambio de propiedad
        End Set
    End Property

    ' Propiedad para el título dinámico de la vista
    Private _currentViewTitle As String
    Public Property CurrentViewTitle As String
        Get
            Return _currentViewTitle
        End Get
        Set(value As String)
            _currentViewTitle = value
            NotifyPropertyChanged(NameOf(CurrentViewTitle)) ' Notifica el cambio de propiedad
        End Set
    End Property

    ' Implementación de INotifyPropertyChanged
    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
    Protected Sub NotifyPropertyChanged(propertyName As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub

    ' Declaración de la función SendMessage
    <DllImport("user32.dll", CharSet:=CharSet.Auto)>
    Public Shared Function SendMessage(ByVal hWnd As IntPtr, ByVal Msg As Integer, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As IntPtr
    End Function

    ' Constantes para mensajes de Windows
    Public Const WM_NCLBUTTONDOWN As Integer = &HA1
    Public Const HT_CAPTION As Integer = &H2

    Public Sub New()
        ' Llama al inicializador de componentes
        InitializeComponent()

        ' Establece el DataContext para enlazar datos
        DataContext = Me

        ' Establece la MaxHeight de la ventana
        Me.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight

        ' Establece el título inicial del menú
        CurrentViewTitle = "Menú"

        ' Simula un login (aquí debes reemplazar con los valores reales del usuario)
        Dim username As String = "admin" ' Reemplaza con el usuario que ha iniciado sesión
        Dim password As String = "admin123" ' Reemplaza con la contraseña del usuario

        ' Recupera el username desde la base de datos
        Dim usernameFromDB As String = GetUsername(username, password)

        ' Asigna el valor directamente al TextBlock
        txtUsername.Text = usernameFromDB
    End Sub

    Private Function GetUsername(username As String, password As String) As String
        Dim usernameFromDB As String = String.Empty

        ' Cadena de conexión desde app.config
        Dim connectionString As String = ConfigurationManager.ConnectionStrings("LoginConnection").ConnectionString

        ' Consulta SQL para obtener el username
        Dim query As String = "SELECT username FROM usuarios WHERE username = @username AND password = @password"

        Using connection As New MySqlConnection(connectionString)
            Using command As New MySqlCommand(query, connection)
                ' Parámetros para evitar SQL injection
                command.Parameters.AddWithValue("@username", username)
                command.Parameters.AddWithValue("@password", password)

                Try
                    connection.Open()
                    Dim reader As MySqlDataReader = command.ExecuteReader()

                    If reader.Read() Then
                        ' Recupera el username
                        usernameFromDB = reader("username").ToString()
                    End If
                Catch ex As Exception
                    ' Maneja errores (puedes mostrar un mensaje al usuario)
                    MessageBox.Show("Error al conectar con la base de datos: " & ex.Message)
                End Try
            End Using
        End Using

        Return usernameFromDB
    End Function

    Private Sub pnlControlBar_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
        ' Permite mover la ventana al arrastrar el panel de control
        Dim helper As New WindowInteropHelper(Me)
        SendMessage(helper.Handle, WM_NCLBUTTONDOWN, New IntPtr(HT_CAPTION), IntPtr.Zero)

        ' Opcional: Enviar un mensaje para mover la ventana (alternativa a DragMove)
        SendMessage(New WindowInteropHelper(Me).Handle, WM_NCLBUTTONDOWN, New IntPtr(HT_CAPTION), IntPtr.Zero)
    End Sub

    Private Sub pnlControlBar_MouseEnter(sender As Object, e As MouseEventArgs)
        Me.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight
    End Sub

    Private Sub btnClose_Click(sender As Object, e As RoutedEventArgs)
        Application.Current.Shutdown()
    End Sub

    Private Sub btnMinimize_Click(sender As Object, e As RoutedEventArgs)
        Me.WindowState = WindowState.Minimized
    End Sub

    Private Sub btnMaximize_Click(sender As Object, e As RoutedEventArgs)
        If Me.WindowState = WindowState.Maximized Then
            ' Si la ventana ya está maximizada, la restaura a su tamaño normal
            Me.WindowState = WindowState.Normal
        Else
            ' Si la ventana no está maximizada, la maximiza
            Me.WindowState = WindowState.Maximized
        End If
    End Sub

    ' Método para cambiar el título de la vista
    Private Sub CambiarTitulo(nuevoTitulo As String)
        CurrentViewTitle = nuevoTitulo
    End Sub

    ' Evento para cambiar a Dashboard
    Private Sub RadioButton_Checked(sender As Object, e As RoutedEventArgs)
        MainContent.Content = New DashboardView()
        CambiarTitulo("Dashboard") ' Cambia el título a "Dashboard"
    End Sub

    ' Evento para cambiar a Home
    Private Sub RadioButton_Checked_1(sender As Object, e As RoutedEventArgs)
        MainContent.Content = New HomeView()
        CambiarTitulo("Home") ' Cambia el título a "Home"
    End Sub

    Private Sub RadioButton_Checked_2(sender As Object, e As RoutedEventArgs)
        MainContent.Content = New ClientesView()
        CambiarTitulo("Clientes") ' Cambia el título a "Home"
    End Sub

    Private Sub RadioButton_Checked_3(sender As Object, e As RoutedEventArgs)
        MainContent.Content = New ProductosView()
        CambiarTitulo("Productos") ' Cambia el título a "Home"
    End Sub
End Class
