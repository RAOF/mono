// cs0188-2.cs: The `this' object cannot be used before all of its fields are assigned to
// Line: 10

struct B
{
	public int a;

	public B (int foo)
	{
		Test ();
	}

	public void Test ()
	{
	}
}