package main

import (
	"context"
	"math/big"
)

func CalculatePI(ctx context.Context, n int) (string, error) {
	pi := big.NewRat(0, 1)
	tempInt := big.NewInt(1)
	term := big.NewRat(0.0, 1.0)

	done := ctx.Done()
	for k := 1; k < 4*n+10; k++ {
		top := big.NewInt(int64(k))
		tempInt = tempInt.Exp(big.NewInt(2), big.NewInt(int64(k)), nil)
		top = top.Mul(top, tempInt)
		tempInt = tempInt.Exp(tempInt.Set(fact(k)), big.NewInt(2), nil)
		top = top.Mul(top, tempInt)
		bottom := fact(2 * k)
		term := term.SetFrac(top, bottom)
		pi.Add(pi, term)

		select {
		case <-done:
			return "", ctx.Err()
		default:
		}
	}

	pi = pi.Add(pi, big.NewRat(-3, 1))
	return pi.FloatString(n), nil
}

func fact(a int) *big.Int {
	b := big.NewInt(1)
	for i := a; i > 1; i-- {
		b = b.Mul(b, big.NewInt(int64(i)))
	}
	return b
}
