Imports System.IO
Imports System.Net
Imports System.Web
Imports System.Web.Services
Imports Gif.Components

Public Class Handler
    Implements System.Web.IHttpHandler

    Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

        Dim url As String
        Dim q = context.Request.QueryString("q")

        Dim r = New Regex("^[0-9A-Za-z\-]+$")
        If r.IsMatch(q) Then
            url = "https://identicons.github.com/" & q & ".png"
        Else
            context.Response.StatusCode = HttpStatusCode.BadRequest
            Exit Sub
        End If

        ' Read Icon
        Dim game = New LifeGame
        Try
            game.LoadIcon(url)

        Catch webEx As WebException
            Dim res = DirectCast(webEx.Response, HttpWebResponse)
            context.Response.StatusCode = res.StatusCode
            Exit Sub
        End Try

        ' Create GIF
        Dim e = New AnimatedGifEncoder()
        Dim fs = New FileStream(Path.GetTempFileName, FileMode.Create)

        e.Start(fs)
        e.SetDelay(500)
        e.SetRepeat(0)

        Dim count = 0
        Do
            e.AddFrame(game.DrawIcon)

            count += 1
        Loop While game.StepNext() AndAlso count < 30 ' max 30 frame

        e.AddFrame(game.DrawIcon)
        e.Finish()

        ' Create Response
        Dim ms = New MemoryStream

        fs.Position = 0
        fs.CopyTo(ms)
        fs.Close()

        ' for Cache
        context.Response.Cache.SetCacheability(HttpCacheability.Public)
        context.Response.Cache.SetExpires(DateTime.Now.AddDays(1))
        context.Response.Cache.SetMaxAge(TimeSpan.FromDays(30))
        context.Response.AddHeader("Last-Modified", Now.ToLongDateString)

        ' Result
        context.Response.ContentType = "image/gif"
        context.Response.BinaryWrite(ms.ToArray)

    End Sub

    ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return True
        End Get
    End Property

End Class