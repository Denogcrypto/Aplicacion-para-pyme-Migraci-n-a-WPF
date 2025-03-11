Imports LiveCharts
Imports LiveCharts.Wpf
Imports MySql.Data.MySqlClient
Imports System.Configuration ' Importar System.Configuration

Public Class DashboardView
    Public Sub New()
        ' Llamada al inicializador de componentes
        InitializeComponent()

        ' Obtener el total de clientes desde la base de datos
        Dim totalClientes As Integer = ObtenerTotal("clientes")
        Dim totalProductos As Integer = ObtenerTotal("productos")

        ' Crear una serie de datos con el total de clientes
        Dim seriesClientes As New PieSeries() With {
            .Title = "Clientes",
            .Values = New ChartValues(Of Double) From {totalClientes},
            .DataLabels = True,
            .LabelPosition = PieLabelPosition.InsideSlice,
            .Foreground = New SolidColorBrush(CType(ColorConverter.ConvertFromString("#eff6ff"), Color)),
            .Fill = New SolidColorBrush(CType(ColorConverter.ConvertFromString("#db2777"), Color)) ' Color 
        }

        ' Crear una serie de datos con el total de productos
        Dim seriesProductos As New PieSeries() With {
            .Title = "Productos",
            .Values = New ChartValues(Of Double) From {totalProductos},
            .DataLabels = True,
            .LabelPosition = PieLabelPosition.InsideSlice,
            .Foreground = New SolidColorBrush(CType(ColorConverter.ConvertFromString("#eff6ff"), Color)),
            .Fill = New SolidColorBrush(CType(ColorConverter.ConvertFromString("#fbbf24"), Color)) ' Color 
        }

        ' Limpiar y agregar las series a los gráficos
        ClientesChart.Series.Clear()
        ClientesChart.Series.Add(seriesClientes)

        ProductosChart.Series.Clear()
        ProductosChart.Series.Add(seriesProductos)
    End Sub

    Private Function ObtenerTotal(tabla As String) As Integer
        Dim total As Integer = 0

        ' Cadena de conexión desde app.config
        Dim connectionString As String = ConfigurationManager.ConnectionStrings("LoginConnection").ConnectionString

        ' Query para obtener el total de la tabla especificada
        Dim query As String = $"SELECT COUNT(*) FROM {tabla}"

        ' Conectar a la base de datos y ejecutar la consulta
        Using connection As New MySqlConnection(connectionString)
            Using command As New MySqlCommand(query, connection)
                connection.Open()
                total = Convert.ToInt32(command.ExecuteScalar())
            End Using
        End Using

        Return total
    End Function
End Class