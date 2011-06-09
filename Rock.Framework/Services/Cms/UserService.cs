//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the T4\Model.tt template.
//
//     Changes to this file will be lost when the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Rock.Models.Cms;
using Rock.Repository.Cms;

namespace Rock.Services.Cms
{
    public partial class UserService : Rock.Services.Service
    {
        private IUserRepository _repository;

        public UserService()
			: this( new EntityUserRepository() )
        { }

        public UserService( IUserRepository UserRepository )
        {
            _repository = UserRepository;
        }

        public IQueryable<Rock.Models.Cms.User> Queryable()
        {
            return _repository.AsQueryable();
        }

        public Rock.Models.Cms.User GetUser( int id )
        {
            return _repository.FirstOrDefault( t => t.Id == id );
        }
		
        public User GetUserByApplicationNameAndUsername( string applicationName, string username )
        {
            return _repository.FirstOrDefault( t => t.ApplicationName == applicationName && t.Username == username );
        }
		
        public User GetUserByApplicationNameAndEmail( string applicationName, string email )
        {
            return _repository.FirstOrDefault( t => t.ApplicationName == applicationName && t.Email == email );
        }
		
        public User GetUserByGuid( Guid guid )
        {
            return _repository.FirstOrDefault( t => t.Guid == guid );
        }
		
        public IEnumerable<Rock.Models.Cms.User> GetUsersByPersonId( int? personId )
        {
            return _repository.Find( t => ( t.PersonId == personId || ( personId == null && t.PersonId == null ) ) );
        }
		
        public void AddUser( Rock.Models.Cms.User User )
        {
            if ( User.Guid == Guid.Empty )
                User.Guid = Guid.NewGuid();

            _repository.Add( User );
        }

        public void AttachUser( Rock.Models.Cms.User User )
        {
            _repository.Attach( User );
        }

		public void DeleteUser( Rock.Models.Cms.User User )
        {
            _repository.Delete( User );
        }

        public void Save( Rock.Models.Cms.User User, int? personId )
        {
            List<Rock.Models.Core.EntityChange> entityChanges = _repository.Save( User, personId );

			if ( entityChanges != null )
            {
                Rock.Services.Core.EntityChangeService entityChangeService = new Rock.Services.Core.EntityChangeService();

                foreach ( Rock.Models.Core.EntityChange entityChange in entityChanges )
                {
                    entityChange.EntityId = User.Id;
                    entityChangeService.AddEntityChange ( entityChange );
                    entityChangeService.Save( entityChange, personId );
                }
            }
        }
    }
}
