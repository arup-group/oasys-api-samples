
Imports System.Math

Module Module1

    Sub Main()

        ' add code here

    End Sub

    Function IsPrime(ByVal n As Integer) As Boolean
        Console.WriteLine("Number to be tested = {0}", n)
        ' square root of the number to be tested for primality
        Dim root As Integer = Math.Sqrt(n)

        ' create an array to store known primes
        Dim primes As New System.Collections.ArrayList
        primes.Add(2)
        primes.Add(3)
        Dim remainder As Integer = 0
        Dim nIsPrime = True

        ' trivial case -- eliminate if multiple of 2 or 3
        For Each j In primes
            DivRem(n, j, remainder)
            If Math.Equals(remainder, 0) Then
                nIsPrime = False
                Console.WriteLine("Factor found for {0} : {1}", n, j)
                Return False
            End If
        Next

        For i As Integer = 4 To root ' it's enough to run a loop till the square root of n
            Dim iIsComposite As Boolean = False

            For Each j In primes
                ' check if i is a multiple of primes we already know about
                Math.DivRem(i, j, remainder)
                If Math.Equals(remainder, 0) Then
                    iIsComposite = True
                    Exit For
                End If
            Next

            If iIsComposite Then
                Continue For ' move to next i, we only need to do the division test with primes
            Else
                primes.Add(i) ' OK, i is prime, we want to see if n is a factor of i
            End If

            ' is n a factor of i?
            Math.DivRem(n, i, remainder)
            If Math.Equals(remainder, 0) Then
                nIsPrime = False
                Console.WriteLine("Factor found for {0} : {1}", n, i)
                Exit For
            End If
        Next
        If nIsPrime Then ' we have tried dividing by all primes from 2 to sqrt(n), since none of them factor n, n is prime!
            Console.WriteLine("{0} is prime!", n)
        End If

        Return nIsPrime

    End Function


End Module
