import { Link } from "react-router-dom";
import { useAuth } from "../pages/AuthContext"

function Navbar() {
    // check if user is logged in by validating token
    const { isLoggedIn } = useAuth();
    const { logout } = useAuth();

    return (
        <nav className="navbar navbar-expand-lg navbar-dark bg-dark p-1">
            <div className="container position-relative" style={{ minHeight: "48px"}}>
                {isLoggedIn && (
                    <>
                        <div className="navbar-nav flex-row gap-4 justify-content-center w-100">
                            <Link className="nav-link" to="/products">Products</Link>
                            <Link className="nav-link" to="/cart">Cart</Link>
                            <Link className="nav-link" to="/profile">Profile</Link>
                            
                        </div>

                        <div className="position-absolute end-0 me-3">
                            <Link className="nav-link text-white" onClick={logout} to="/login">Sign Out</Link>
                        </div>
                    </>
                )}

                {!isLoggedIn && (
                    <div className="navbar-nav flex-row gap-3 ms-auto">
                    </div>
                )}
            </div>
        </nav>
    );
}

export default Navbar;