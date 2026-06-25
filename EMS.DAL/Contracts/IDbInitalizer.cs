using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.DAL.Contracts
{
    public interface IDbInitalizer
    {
        void Initilaize();
        void Seed();
    }
}
