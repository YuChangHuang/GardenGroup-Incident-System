/* ============================================================
   Garden Group Incident System - MongoDB seed data
   Runs automatically on first container start (database: GardenGrup).

   Login is by FIRST NAME + password. Passwords are stored as SHA-256
   (hex), matching the app's hashing. Demo accounts (see README):
     Alice / admin123  (supervisor)
     Bob   / desk123   (service desk)
     Carol / user123   (regular employee)
   ============================================================ */

db = db.getSiblingDB('GardenGrup');

// Idempotent: clear existing demo data so re-seeding is clean
db.Employee.deleteMany({});
db.Tickets.deleteMany({});
db.ArchivedTickets.deleteMany({});

db.Employee.insertMany([
  {
    _id: "E0001",
    Password: "240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9", // admin123
    Role: "supervisor",
    Name: { FirstName: "Alice", LastName: "Walker" },
    ContactDetails: { EmailAddress: "alice@gardengroup.local", Location: "Amsterdam", PhoneNumber: "0600000001" }
  },
  {
    _id: "E0002",
    Password: "dc7955dd78ce7026e028c959feec2fcd532cfcb7fab2023a8403a9eae805709c", // desk123
    Role: "service_desk",
    Name: { FirstName: "Bob", LastName: "Chen" },
    ContactDetails: { EmailAddress: "bob@gardengroup.local", Location: "Rotterdam", PhoneNumber: "0600000002" }
  },
  {
    _id: "E0003",
    Password: "e606e38b0d8c19b24cf0ee3808183162ea7cd63ff7912dbb22b5e803286b4446", // user123
    Role: "regular",
    Name: { FirstName: "Carol", LastName: "Diaz" },
    ContactDetails: { EmailAddress: "carol@gardengroup.local", Location: "Amsterdam", PhoneNumber: "0600000003" }
  },
  {
    _id: "E0004",
    Password: "e606e38b0d8c19b24cf0ee3808183162ea7cd63ff7912dbb22b5e803286b4446", // user123
    Role: "regular",
    Name: { FirstName: "David", LastName: "Smith" },
    ContactDetails: { EmailAddress: "david@gardengroup.local", Location: "Utrecht", PhoneNumber: "0600000004" }
  },
  {
    _id: "E0005",
    Password: "dc7955dd78ce7026e028c959feec2fcd532cfcb7fab2023a8403a9eae805709c", // desk123
    Role: "servicedesk_1",
    Name: { FirstName: "Emma", LastName: "Jones" },
    ContactDetails: { EmailAddress: "emma@gardengroup.local", Location: "Rotterdam", PhoneNumber: "0600000005" }
  }
]);

var now = new Date();
function daysFromNow(d) { return new Date(now.getTime() + d * 24 * 60 * 60 * 1000); }

db.Tickets.insertMany([
  {
    _id: "T0001",
    EmployeeID: "E0003",
    Subject: "Laptop won't boot",
    Description: "My work laptop shows a black screen after the latest update and will not start.",
    type: "Hardware Request",
    priority: "High",
    DateTimeReport: daysFromNow(-2),
    deadline: daysFromNow(1),
    Status: "Open",
    workedBy: []
  },
  {
    _id: "T0002",
    EmployeeID: "E0004",
    Subject: "Cannot access shared drive",
    Description: "Access to the Finance shared drive is denied since this morning.",
    type: "Access Request",
    priority: "Medium",
    DateTimeReport: daysFromNow(-1),
    deadline: daysFromNow(2),
    Status: "Open",
    workedBy: []
  },
  {
    _id: "T0003",
    EmployeeID: "E0003",
    Subject: "Install Adobe Reader",
    Description: "Please install Adobe Acrobat Reader on my workstation.",
    type: "Software",
    priority: "Low",
    DateTimeReport: daysFromNow(-5),
    deadline: daysFromNow(-2),
    Status: "Resolved",
    workedBy: []
  },
  {
    _id: "T0004",
    EmployeeID: "E0004",
    Subject: "Email not syncing on phone",
    Description: "Outlook on my phone stopped syncing new messages yesterday.",
    type: "Software",
    priority: "Medium",
    DateTimeReport: daysFromNow(-1),
    deadline: daysFromNow(3),
    Status: "Open",
    workedBy: []
  },
  {
    _id: "T0005",
    EmployeeID: "E0005",
    Subject: "Monitor flickering",
    Description: "Second monitor flickers intermittently; cable already replaced.",
    type: "Hardware Request",
    priority: "Low",
    DateTimeReport: daysFromNow(-8),
    deadline: daysFromNow(-4),
    Status: "Closed",
    workedBy: []
  },
  {
    _id: "T0006",
    EmployeeID: "E0003",
    Subject: "VPN connection keeps dropping",
    Description: "The VPN disconnects every few minutes when working from home.",
    type: "Network",
    priority: "High",
    DateTimeReport: daysFromNow(0),
    deadline: daysFromNow(1),
    Status: "Open",
    workedBy: []
  }
]);

print("Garden Group seed data loaded: " +
      db.Employee.countDocuments() + " employees, " +
      db.Tickets.countDocuments() + " tickets.");
