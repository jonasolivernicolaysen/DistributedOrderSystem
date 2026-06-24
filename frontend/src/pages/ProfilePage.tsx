import { useState, useEffect } from "react";

interface UserDetails {
    username: string,
    email: string,
    balance: number
}

interface Product {
    productId: string,
    name: string,
    description: string,
    price: number
}
function ProfilePage() {

    // state
    const [details, setDetails] = useState<UserDetails>();
    const [products, setProducts] = useState<Product[]>([]);
    const [selectedProduct, setSelectedProduct] = useState<Product | null>(null);

    const [showUpdateListing, setShowUpdateListing] = useState(false);
    const [editName, setEditName] = useState("");
    const [editDescription, setEditDescription] = useState("");
    const [editPrice, setEditPrice] = useState(0);

    const [showUpdateStock, setShowUpdateStock] = useState(false);
    const [newStock, setNewStock] = useState(0);

    const [showDelete, setShowDelete] = useState(false);


    // functions


    const updateListing = async (productId: string) => {
        const token = localStorage.getItem("token");

        const response = await fetch(
            `https://localhost:7144/api/products/${productId}`,
            {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${token}`
                },
                body: JSON.stringify({
                    productId: selectedProduct.productId,
                    name: editName,
                    description: editDescription,
                    price: editPrice
                }) 
            });


        if (!response.ok) {
            alert(`Request failed: ${response.status}`);
            return;
        }
        const data = await response.json();
        alert("Product listing updated successfully")
        return data;
    }

    const updateStock = async (productId: string, updatedStock: number) => {
        const token = localStorage.getItem("token");

        const response = await fetch(
            `https://localhost:7144/api/inventory/${productId}`,
            {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${token}`
                },
                body: JSON.stringify({
                    updatedStock
                })
            });

        if (!response.ok) {
            alert(`Request failed: ${response.status}`);
            return;
        }
        const data = await response.json();
        alert("Stock updated successfully")
        return data;
    }

    const deleteListing = async (productId: string) => {
        const token = localStorage.getItem("token");

        const response = await fetch(
            `https://localhost:7144/api/products/${productId}`,
            {
                method: "DELETE",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${token}`
                },
                body: JSON.stringify({
                    productId
                })
            });

        if (!response.ok) {
            alert(`Request failed: ${response.status}`);
            return;
        }
        const data = await response.json();
        alert("Successfully deleted product")
        return data;
    }



    useEffect(() => {
        const getUserDetails = async () => {
            const token = localStorage.getItem("token");

            // send request to authservice getUserDetails endpoint
            const response = await fetch(
                "https://localhost:7144/api/auth/profile",
                {
                    method: "GET",
                    headers: {
                        "Content-Type": "application/json",
                        "Authorization": `Bearer ${token}`
                    }
                });

            if (!response.ok) {
                alert(`Request failed: ${response.status}`);
                return;
            }
            const data = await response.json();
            setDetails(data)
            return data;
        }

        getUserDetails();
    }, [])

    useEffect(() => {
        const getUserProducts = async () => {
            const token = localStorage.getItem("token");

            const response = await fetch(
                "https://localhost:7144/api/products/me",
                {
                    method: "GET",
                    headers: {
                        "Content-Type": "application/json",
                        "Authorization": `Bearer ${token}`
                    }
                });

            if (!response.ok) {
                alert(`Request failed: ${response.status}`);
                return;
            }
            const data = await response.json();
            setProducts(data)
            return data;
        }

        getUserProducts();
    }, [])



    // html
    return (
        <div>
            <p>Username</p>
            <p>Email</p>
            <p>Balance</p>
            <p>{details?.username}</p>
            <p>{details?.email}</p>
            <p>{details?.balance}</p>
            <p>My products:</p>

            <div className="container mt-3">
                {products.map((product: Product) => (
                    <div
                        key={product.productId}
                        className="card mb-2"
                    >
                        <div className="card-body">
                            <h5>{product.name}</h5>
                            <p>{product.description}</p>
                            <strong>{product.price}</strong>

                            <button
                                className="btn btn-secondary"
                                onClick={() =>
                                    setSelectedProduct(
                                        selectedProduct === product ? null : product
                                    )}
                            >Edit</button>

                            {selectedProduct === product && (
                                <div className="card-footer">

                                    <div className="d-grid gap-2">

                                        <button
                                            className="btn btn-primary me-2"
                                            onClick={() => {
                                                setSelectedProduct(product);

                                                setEditName(product.name);
                                                setEditDescription(product.description);
                                                setEditPrice(product.price);

                                                setShowUpdateListing(true);
                                            }}
                                        >
                                            Update Listing
                                        </button>

                                        <button
                                            className="btn btn-warning me-2"
                                            onClick={() => {
                                                setSelectedProduct(product);
                                                setShowUpdateStock(true);
                                            }}
                                        >
                                            Update Stock
                                        </button>

                                        <button
                                            className="btn btn-danger"
                                            onClick={() => {
                                                setSelectedProduct(product);
                                                setShowDelete(true);
                                            }}
                                        >
                                            Delete Listing
                                        </button>

                                    </div>

                                </div>
                            )}
                        </div>
                    </div>
                ))}
            </div>

            
            {showUpdateListing && (
                <div className="modal d-block">
                    <div className="modal-dialog">
                        <div className="modal-content">

                            <div className="modal-header">
                                <h5>Update Listing</h5>
                            </div>

                            <div className="modal-body">

                                <input
                                    className="form-control mb-2"
                                    value={editName}
                                    onChange={(e) =>
                                        setEditName(e.target.value)
                                    }
                                />

                                <textarea
                                    className="form-control mb-2"
                                    value={editDescription}
                                    onChange={(e) =>
                                        setEditDescription(e.target.value)
                                    }
                                />

                                <input
                                    type="number"
                                    className="form-control"
                                    value={editPrice}
                                    onChange={(e) =>
                                        setEditPrice(Number(e.target.value))
                                    }
                                />

                            </div>

                            <div className="modal-footer">

                                <button
                                    className="btn btn-secondary"
                                    onClick={() =>
                                        setShowUpdateListing(false)
                                    }
                                >
                                    Cancel
                                </button>

                                <button
                                    className="btn btn-primary"
                                    onClick={() =>
                                        updateListing(selectedProduct.productId)}
                                >
                                    Save
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            )}

            {showUpdateStock && (
                <div className="modal d-block">
                    <div className="modal-dialog">
                        <div className="modal-content">

                            <div className="modal-header">
                                <h5>Update Stock</h5>
                            </div>

                            <div className="modal-body">

                                <input
                                    type="number"
                                    className="form-control"
                                    value={newStock}
                                    onChange={(e) =>
                                        setNewStock(Number(e.target.value))
                                    }
                                />

                            </div>

                            <div className="modal-footer">

                                <button
                                    className="btn btn-secondary"
                                    onClick={() =>
                                        setShowUpdateStock(false)
                                    }
                                >
                                    Cancel
                                </button>

                                <button
                                    className="btn btn-warning"
                                    onClick={() =>
                                        updateStock(selectedProduct.productId, newStock)}
                                >
                                    Save
                                </button>

                            </div>

                        </div>
                    </div>
                </div>
            )}

            {showDelete && (
                <div className="modal d-block">
                    <div className="modal-dialog">
                        <div className="modal-content">

                            <div className="modal-header">
                                <h5>Delete Listing</h5>
                            </div>

                            <div className="modal-body">

                                Are you sure you want to delete:

                                <strong>
                                    {" "}
                                    {selectedProduct?.name}
                                </strong>
                                ?

                            </div>

                            <div className="modal-footer">

                                <button
                                    className="btn btn-secondary"
                                    onClick={() =>
                                        setShowDelete(false)
                                    }
                                >
                                    Cancel
                                </button>

                                <button
                                    className="btn btn-danger"
                                    onClick={() =>
                                        deleteListing(selectedProduct.productId)}
                                >
                                    Delete
                                </button>

                            </div>

                        </div>
                    </div>
                </div>
            )}

        </div>
    );
}


export default ProfilePage;