/**
* @author Stefano Ceccotti
*/

using Funwap.expression;
using Funwap.parser;
using Funwap.statements;

public class While : IStatement
{
	/* The guard. */
	private IExpr e;
	/* The block. */
	private Block b;

	public While( IExpr e, Block b )
	{
		this.e = e;
		this.b = b;
	}

	/** Returns the guard of the while. */
	public IExpr getGuard() {
		return e;
	}

	/** Returns the block of the while. */
	public Block getBlock() {
		return b;
	}
	
	public int getTAG() {
		return Token.T_WHILE;
	}
}