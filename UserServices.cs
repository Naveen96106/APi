using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Volo.Abp.Identity.Settings.IdentitySettingNames;
using static Volo.Abp.UI.Navigation.DefaultMenuNames.Application;

namespace UserManagement2
{
    public class UserServices : ApplicationService, ITransientDependency
    {
        private readonly IRepository<UserDetails> _UserRepo;
        private readonly IRepository<ContactDetails> _ContactRepo;
        private readonly IRepository<RoleDetails> _RoleRepo;

        public UserServices(IRepository<UserDetails> UserRepo,
            IRepository<ContactDetails> ContactRepo,
            IRepository<RoleDetails> RoleRepo)
        {
            _UserRepo = UserRepo;
            _ContactRepo = ContactRepo;
            _RoleRepo = RoleRepo;
        }

        public async Task<UserDetailsDto> CreateUser(UserDetailsDto input)
        {
            //tried to automatically update id from one table to another  

            /*var Data = await _RoleRepo.GetListAsync();
            UserDetails obj = new UserDetails();
            obj.Roleid = Data.Id;*/

            try
            {
                UserDetails dat2 = new UserDetails();
                var data11 = await _UserRepo.AnyAsync(o => o.Email == input.Email && o.Name==input.Name);
                if (!data11)
                {
                    var data1 = ObjectMapper.Map<UserDetailsDto, UserDetails>(input);
                    dat2 = await _UserRepo.InsertAsync(data1);


                }

                var d = ObjectMapper.Map<UserDetails, UserDetailsDto>(dat2);

                return d;
            }

            catch(Exception ex)
            {
                throw new UserFriendlyException("User Already Exist",ex.Message);
            }
            
        }

        public async Task<RoleDetails> CreateRole(RoleDetailsDto input)
        {


            //initiall method to create an entry in the table

            /*var data1 = ObjectMapper.Map<RoleDetailsDto, RoleDetails>(input);
            var Data  = await _RoleRepo.GetListAsync();

            var dat2 = await _RoleRepo.InsertAsync(data1);
            var dat3 = ObjectMapper.Map<RoleDetails, RoleDetailsDto>(dat2);
            return dat3;*/


            // linq method to check wether Role is already present in the Collection


            RoleDetails data = new RoleDetails();
            var data1 = await _RoleRepo.AnyAsync(x=>x.Role==input.Role);
            if (!data1) {
                
                data = await _RoleRepo.InsertAsync(ObjectMapper.Map<RoleDetailsDto,RoleDetails>(input));
                
            }

            return data;
            
            
                        

                //for loop method to check wether an entry is already present in the table

                //int flag=0;
                //foreach (var d in data)
                //{
                    
                //    if (d.Role == name)
                //    {
                //        flag = 1;
                //        break;
                //    }
                //    else
                //    {
                //        continue;
                //    }
                //}
                //RoleDetails obj = new RoleDetails();
                //if (flag != 1)
                //{
                    
                //    obj.Role = name;
                //    /*var d = await _RoleRepo.InsertAsync(o => o.Role=name);
                //    return ObjectMapper.Map<UserDetails,UserDetailsDto>(d);*/
                //    await _RoleRepo.InsertAsync(obj);
                    
                //    /*User newUser = new User();
                //    newUser.Name = "John";
                //    await _dbc.AddAsync(newUser);
                //    await _dbc.SaveChangesAsync();*/

                //}
               
                //return obj;

            

            
        }
        
        public async Task<ContactDetailsDto> CreateContact(ContactDetailsDto input)
        {
                try
                {
                /*var data1 = ObjectMapper.Map<ContactDetailsDto, ContactDetails>(input);
                var dat2 = await _ContactRepo.InsertAsync(data1);
                var dat3 = ObjectMapper.Map<ContactDetails, ContactDetailsDto>(dat2);
                return dat3;*/

                ContactDetails obj = new ContactDetails();
                var data = await _ContactRepo.AnyAsync(x => x.Phone==input.Phone);
                if (!data)
                {

                    obj = await _ContactRepo.InsertAsync(ObjectMapper.Map<ContactDetailsDto, ContactDetails>(input)); 
                }
                var obj1 = ObjectMapper.Map<ContactDetails, ContactDetailsDto>(obj);
                return obj1;
                }

                catch (Exception ex)
                {

                    throw new UserFriendlyException("Something went wrong" + ex.Message);
                }
        }

        public async Task<List<RoleDetails>> GetRole(string letter)
        {
            try
            {
                var data = await _RoleRepo.GetListAsync();
                return data.Where(x => x.Role.StartsWith(letter)).ToList();
            }
            catch(Exception e)
            {
                throw new UserFriendlyException("No Role Exist in Such Name" + e.Message);
            }
        }

        public async Task<List<UserDetails>> GetUser(string letter)
        {
            var data = await _UserRepo.GetListAsync();
            return data.Where(x=>x.Name.Contains(letter)).ToList();
        }


        [HttpGet]
        public async Task<List<FetchTotalDetails>> FetchUserDetails(string letter)
        {
            var userdata = await _UserRepo.GetListAsync();
            var roledata = await _RoleRepo.GetListAsync();
            var contactdata = await _ContactRepo.GetListAsync();
            var data = (from d1 in userdata
                        join d2 in roledata on d1.Roleid equals d2.Id
                        join d3 in contactdata on d1.Id equals d3.Empid
                        select new FetchTotalDetails
                        {

                            Name = d1.Name,
                            Email = d1.Email,
                            CabRequired = d1.CabRequired,
                            Foodrequired = d1.Foodrequired,
                            Role = d2.Role,
                            City = d3.City,
                            PostalCode = d3.PostalCode,
                            Region = d3.Region,
                            Country = d3.Country,
                            Roleid =d2.Id,
                            Empid=d1.Id,
                            Contactid=d3.Id

                        }).ToList();
            //return data.FirstOrDefault(o=>o.Name.StartsWith("N"));
            return data.Where(o => o.Name.Contains(letter)).ToList();
            /*foreach (var d in data)
            {
                return ObjectMapper.Map<UserDetails,FetchTotalDetails>(d);
            }*/


        }

        public async Task<CreateUpdateUserDto> UpdateUser(Guid id,CreateUpdateUserDto input)
        {
            var data = await _UserRepo.GetAsync(o => o.Id == id);
            data.Name = input.Name;
            data.Email = input.Email;
            data.CabRequired = input.CabRequired;
            data.Foodrequired = input.Foodrequired;
            await _UserRepo.UpdateAsync(data);
            return ObjectMapper.Map<UserDetails,CreateUpdateUserDto>(data);
        }
        public async Task<CreateUpdateRoleDto> UpdateRole(Guid id, CreateUpdateRoleDto input)
        {
            var data = await _RoleRepo.GetAsync(o => o.Id == id);
            data.Role = input.Role;
            
            await _RoleRepo.UpdateAsync(data);
            return ObjectMapper.Map<RoleDetails, CreateUpdateRoleDto>(data);
        }

        public async Task<CreateUpdateContactsDto> UpdateContact(Guid id, CreateUpdateContactsDto input)
        {
            var data = await _ContactRepo.GetAsync(o => o.Id == id);
            data.City = input.City == "string" ? data.City = data.City : data.City = input.City;
            data.Region = input.Region == "string" ? data.Region = data.Region : data.Region = input.Region;
            data.PostalCode = input.PostalCode == "string" ? data.PostalCode = data.PostalCode : data.PostalCode = input.PostalCode;
            data.Phone = input.Phone == "string" ? data.Phone = data.Phone : data.Phone = input.Phone;
            data.Country = input.Country == "string" ? data.Country = data.Country : data.Country = input.Country;

            /*data.City = input.City;
            data.Region = input.Region;
            data.PostalCode = input.PostalCode;
            data.Country = input.Country;
            data.Phone = input.Phone;*/

            await _ContactRepo.UpdateAsync(data);
            return ObjectMapper.Map<ContactDetails, CreateUpdateContactsDto>(data);
        }
        public async Task<ContactDetails> GetContactDetails(string phone)
        {
            try
            {
                var data = await _ContactRepo.GetAsync(o=>o.Phone==phone);
                
                return data;
            }
            catch (Exception e)
            {
                throw new UserFriendlyException("No data Exist in Such Name" + e.Message);
            }
        }



    }
    }

