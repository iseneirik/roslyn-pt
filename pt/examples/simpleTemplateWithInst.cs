/* WITH ONLY ONE TEMPLATE */
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

/* WITH MULTIPLE TEMPLATES */
namespace N 
{
	template T1
	{
		class A
		{
			int i;
			int j;
		}
	}

	template T2
	{
		class A
		{
			int k;
		}
	}

	inst T1, T2
	{
		T1.A ~> AA (i ~> ii, j ~> jj);
		T2.A ~> AA (k ~> kk);

		AA
		{
			int ll;
		}
	}
}