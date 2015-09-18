Imports System.Drawing
Imports System.Net

Public Class LifeGame

    Enum CellLife
        Dead = 0
        Live = 1
    End Enum

    Private _Cells(8, 8) As CellLife ' 9x9 pixel

    ReadOnly Property Cells2 As CellLife(,)
        Get
            Return _Cells
        End Get
    End Property

    Property BlankColor As Color = Color.FromArgb(&HF0, &HF0, &HF0)

    Property IconColor As Color

    ''' <summary>
    ''' GitHub アイコン読み込み
    ''' </summary>
    ''' <param name="url"></param>
    Sub LoadIcon(url As String)

        Dim bmp As Bitmap

        Using client = New WebClient
            Using stream = client.OpenRead(url)

                bmp = New Bitmap(stream)

            End Using
        End Using

        _Cells.Initialize()
        Dim blankArgb = BlankColor.ToArgb

        ' Create cells
        For y = 0 To 4
            For x = 0 To 4
                Dim p = bmp.GetPixel(70 * x + 70, 70 * y + 70)
                If p.ToArgb = blankArgb Then
                    _Cells(y + 2, x + 2) = CellLife.Dead
                Else
                    _Cells(y + 2, x + 2) = CellLife.Live
                    IconColor = p
                End If
            Next
        Next

    End Sub

    ''' <summary>
    ''' 1ステップ進める
    ''' </summary>
    ''' <returns>セルの形が変更されたかどうか</returns>
    Function StepNext() As Boolean
        Dim nextCells(8, 8) As CellLife

        For y = 1 To 7
            For x = 1 To 7
                Dim count = CountLiveCells(x, y)

                If _Cells(y, x) = CellLife.Dead Then
                    If count = 3 Then
                        ' 誕生
                        nextCells(y, x) = CellLife.Live
                    End If
                Else
                    If count = 2 OrElse count = 3 Then
                        ' 生存
                        nextCells(y, x) = CellLife.Live
                    Else
                        ' 死滅, 過密
                        nextCells(y, x) = CellLife.Dead
                    End If
                End If

            Next
        Next

        Dim changed = True
        If _Cells.Cast(Of Integer).SequenceEqual(nextCells.Cast(Of Integer)) Then
            changed = False
        End If

        _Cells = nextCells

        Return changed
    End Function

    ''' <summary>
    ''' 周囲の生存数
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="y"></param>
    ''' <returns></returns>
    Private Function CountLiveCells(x As Integer, y As Integer) As Integer
        Dim sum = 0
        For y2 = y - 1 To y + 1
            For x2 = x - 1 To x + 1
                If x2 = x AndAlso y2 = y Then
                    Continue For
                End If

                sum += _Cells(y2, x2)
            Next
        Next
        Return sum
    End Function

    ''' <summary>
    ''' アイコン生成
    ''' </summary>
    ''' <returns></returns>
    Function DrawIcon() As Bitmap
        Dim bmp = New Bitmap(420, 420)
        Dim g = Graphics.FromImage(bmp)
        g.FillRectangle(New SolidBrush(BlankColor), 0, 0, bmp.Width, bmp.Height)

        Dim brush = New SolidBrush(IconColor)
        For y = 0 To 6
            For x = 0 To 6
                If _Cells(y + 1, x + 1) = CellLife.Dead Then
                    Continue For
                End If

                g.FillRectangle(brush, 70 * x - 35, 70 * y - 35, 71, 71)
            Next
        Next

        g.Dispose()
        Return bmp
    End Function

End Class
