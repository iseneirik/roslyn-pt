

namespace N
{
	template T 
	{
		class A
		{
			int i;
			int j;
			int k;
		}

		class B
		{
			int n;
			int p;
		}
	}

	inst T 
	{
		// First, rename clause
		A ~> AA (i ~> ii, j ~> jj, k ~> kk);
		B ~> BB (n ~> nn, p ~> pp);

		// Then, adds clause
		AA
		{
			int ll;
		};

		BB
		{
			int qq;
		};
	}
	
}